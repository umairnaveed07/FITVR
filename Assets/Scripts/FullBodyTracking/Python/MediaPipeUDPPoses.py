import cv2
import mediapipe as mp
import time
import math

import socket
import struct
import os

import numpy as np

import subprocess
import re
import socket
import sys

UDP_IP = "127.0.0.1"
UDP_PORT = 8081

#For multiple video. Set the video file path. Make sure that you are referncing two different .mp4 files.
#USE_CAMS = ["Downloads/pose_segmentation.mp4","Downloads/pose_segmentation_1.mp4"]

#For single cam
USE_CAMS = [0]

#For multiple cam with cam with id=1 as focus cam
#USE_CAMS = [1,0]

#Set to true for Windows while using multiple webcam
#USE_CAMS_DSHOW = [False,False]

#Set to true for Windows while using single webcam
USE_CAMS_DSHOW = [True]

MIN_CONFIDENCE_FACTOR = 0.5
MIN_TRACKING_CONFIDENCE = 0.5
MIN_VISIBILITY = 0.5

FRAME_SHAPE = [480, 640]
MODEL_COMPLEXITY = 1

FONT			= cv2.FONT_HERSHEY_SIMPLEX
FONT_SCALE		= 1
FONT_COLOR		= (255,255,255)
FONT_THICKNESS	= 2
FONT_LINETYPE	= 2
DRAW_DEBUG		= True
RUNNING			= True
CAM_ID_TO_INDEX = {}

for i in range(len(USE_CAMS)):
	CAM_ID_TO_INDEX[USE_CAMS[i]] = i


mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_pose = mp.solutions.pose


sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP


def send_landmarks(mp_results, fps, cam_id):
	idx = 0
	packet_count = 0
	
	struct_datatype = "bddd"
	packed_entries = []
	
	if mp_results.pose_world_landmarks != None:
		for landmark in mp_results.pose_world_landmarks.landmark:
			
			if landmark.visibility > MIN_VISIBILITY:
				packed_entries.append(idx)
				packed_entries.append(landmark.x)
				packed_entries.append(landmark.y)
				packed_entries.append(landmark.z)
				packet_count = packet_count + 1

			idx = idx + 1
	
	if packet_count > 0:
		timestamp = time.time()
	
		struct_datatype = "<" + "d" + "d" + "d" + "d" + struct_datatype*packet_count
		udp_data_array = bytearray(struct.pack(struct_datatype, cam_id,len(USE_CAMS), timestamp,fps, *packed_entries))
		
		sock.sendto(udp_data_array , (UDP_IP, UDP_PORT))
		
		
	return packet_count

def use_single_cam(cam_id):
	global RUNNING
	
	if USE_CAMS_DSHOW[0]:
		cap = cv2.VideoCapture(cam_id, cv2.CAP_DSHOW)
	else:
		cap = cv2.VideoCapture(cam_id)
		
	pose = mp_pose.Pose(min_detection_confidence=MIN_CONFIDENCE_FACTOR, min_tracking_confidence=MIN_TRACKING_CONFIDENCE, model_complexity=MODEL_COMPLEXITY)
	last_fps = 0
	
	
	while cap.isOpened() and RUNNING:
		
		success, image = cap.read()
		if not success:
			cap.set(cv2.CAP_PROP_POS_FRAMES, 0)
			print("Replay")
			continue

		c_ticks = time.time()


		pose_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
		
		pose_image.flags.writeable = False
		results = pose.process(pose_image)
		pose_image.flags.writeable = True
		
		last_fps = 1.0 / (time.time() - c_ticks)
		
	
		packet_count = send_landmarks(results, last_fps, 0)
		last_fps = str(math.floor(last_fps) ) + " - " + str(packet_count)
		
		if DRAW_DEBUG:
			mp_drawing.draw_landmarks(
				image,
				results.pose_landmarks,
				mp_pose.POSE_CONNECTIONS,
				landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
		
			cv2.putText(image,str(last_fps), (20,40), FONT, FONT_SCALE,FONT_COLOR,FONT_THICKNESS,FONT_LINETYPE)
		
		
		cv2.imshow('Mediapipe_Camera_Preview', image)
		
		
		if cv2.waitKey(1) & 0xFF == 27:
			RUNNING = False
			break
		elif cv2.getWindowProperty('Mediapipe_Camera_Preview',cv2.WND_PROP_VISIBLE) < 1:
			RUNNING = False
			break
			  
	cap.release()
	cv2.destroyAllWindows()
	
	
def use_multiple_cams(cam_ids):
	global RUNNING

	caps = []
	poses = []
	
	frames = []
	last_fps = 0
	
	for i in range(len(cam_ids)):
	
		if USE_CAMS_DSHOW[i]:
			cap = cv2.VideoCapture(cam_ids[i], cv2.CAP_DSHOW)
		else:
			cap = cv2.VideoCapture(cam_ids[i])
	
		caps.append(cap)
		poses.append(mp_pose.Pose(min_detection_confidence=MIN_CONFIDENCE_FACTOR, min_tracking_confidence=MIN_TRACKING_CONFIDENCE, model_complexity=MODEL_COMPLEXITY) )
	
	for cap in caps:
		cap.set(3, FRAME_SHAPE[1])
		cap.set(4, FRAME_SHAPE[0])
		
		
	while RUNNING:
	
		frames = []
		packet_counts = []
		
		
		c_ticks = time.time()
		
		for i in range(len(cam_ids)):
			ret, frame = caps[i].read()
			
			#if ret == False:
				#print("Error in reading frame from webcam")
				#break
			if not ret:
				caps[i].set(cv2.CAP_PROP_POS_FRAMES, 0)
				print("Replay")
				continue	
			
			pose_image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
			
			pose_image.flags.writeable = False
			pose_result = poses[i].process(pose_image)
			pose_image.flags.writeable = True
			
			if DRAW_DEBUG:
				mp_drawing.draw_landmarks(
					frame,
					pose_result.pose_landmarks,
					mp_pose.POSE_CONNECTIONS,
					landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
			
			
			packet_counts.append( send_landmarks(pose_result, last_fps, i ) )
			frames.append(frame)
			
		last_fps = 1.0 / (time.time() - c_ticks)
		#print(last_fps)
		
			
		
		for i in range(len(frames)):
		
			if DRAW_DEBUG:
				cv2.putText(frames[i],str(math.floor(last_fps)) + " - " + str(packet_counts[i] ), (20,40), FONT, FONT_SCALE,FONT_COLOR,FONT_THICKNESS,FONT_LINETYPE)
		
		
			win_name = 'Mediapipe_Camera_Preview_' + str(cam_ids[i])
			cv2.imshow(win_name,frames[i])

	
	
		if cv2.waitKey(1) & 0xFF == 27:
			RUNNING = False
			break
			
		for i in range(len(frames)):
		
			win_name = 'Mediapipe_Camera_Preview_' + str(cam_ids[i])

			if cv2.getWindowProperty(win_name,cv2.WND_PROP_VISIBLE) < 1:
				RUNNING = False
				break
			
			
	cv2.destroyAllWindows()

def close_mediapipe_client():
	global RUNNING
	RUNNING = False
	

def run_mediapipe_client(target_ip, used_cams, used_dshow):
	global UDP_IP
	global USE_CAMS
	global USE_CAMS_DSHOW
	global RUNNING

	UDP_IP = target_ip
	USE_CAMS = used_cams
	USE_CAMS_DSHOW = used_dshow
	RUNNING = True
	
	print("Starting pose detection with the following parameter")
	print(UDP_IP)
	print(USE_CAMS)
	print(USE_CAMS_DSHOW)
	print("--------------")
	
	if len(USE_CAMS) <= 0:
		print("Error invalid cam-ids given")
	elif len(USE_CAMS) == 1:
		use_single_cam(USE_CAMS[0])
	else:
		use_multiple_cams(USE_CAMS)
	

if __name__ == '__main__':	
	run_mediapipe_client(UDP_IP, USE_CAMS, USE_CAMS_DSHOW)
