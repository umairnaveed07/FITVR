import cv2
import mediapipe as mp
import time
import math

from pykinect2 import PyKinectV2
from pykinect2.PyKinectV2 import *
from pykinect2 import PyKinectRuntime

import socket
import struct
import os

import numpy as np
import ctypes

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

SMOOTHING_FRAME_COUNT = 8
SMOOTHING_FRAMES = []

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
KINECT_DEPTH_SHAPE = [512, 424]
KINECT_IMAGE_SHAPE = [1920, 1080]
KINECT_IMAGE_MAX_SIZE = 1920*1080

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


KINECT = PyKinectRuntime.PyKinectRuntime(PyKinectV2.FrameSourceTypes_Color | PyKinectV2.FrameSourceTypes_Depth)


def send_landmarks(mp_results, fps, cam_id):
	packet_count = 0
	
	struct_datatype = "bddd"
	packed_entries = []
	
	for i in range(33): 
		landmark = mp_results[i]
		
		if landmark['visibility'] > MIN_VISIBILITY:
			packed_entries.append(i)
			packed_entries.append(landmark['x'])
			packed_entries.append(landmark['y'])
			packed_entries.append(landmark['z'])
			packet_count = packet_count + 1
	
	if packet_count > 0:
		timestamp = time.time()
	
		struct_datatype = "<" + "d" + "d" + "d" + "d" + struct_datatype*packet_count
		udp_data_array = bytearray(struct.pack(struct_datatype, cam_id,len(USE_CAMS), timestamp,fps, *packed_entries))
		
		sock.sendto(udp_data_array , (UDP_IP, UDP_PORT))
		
		
	return packet_count


def smooth_landmarks(landmarks):

	SMOOTHING_FRAMES.append(landmarks)
	
	if len(SMOOTHING_FRAMES) <= SMOOTHING_FRAME_COUNT:
		return landmarks
	
	
	del SMOOTHING_FRAMES[0]
	result = []
	
	best_frame = 0
	
	for i in range(33):
	
		if SMOOTHING_FRAMES[0][i]['visibility'] < MIN_VISIBILITY:
			found_good_frame = False
			
			for j in range(SMOOTHING_FRAME_COUNT-1):
			
			   if SMOOTHING_FRAMES[j][i]['visibility'] < MIN_VISIBILITY:
					continue

			   found_good_frame = True
			   best_frame = j
			   break
			   
			if found_good_frame == False:
				smooth_pos = {}
				smooth_pos['visibility'] = 0.0
				
				result.append(smooth_pos)
				continue

	
		avg_x = SMOOTHING_FRAMES[best_frame][i]['x']
		avg_y = SMOOTHING_FRAMES[best_frame][i]['y']
		avg_z = SMOOTHING_FRAMES[best_frame][i]['z']
		
		divider = 1
	
		for j in range(best_frame+1, SMOOTHING_FRAME_COUNT-1):
		
			if SMOOTHING_FRAMES[j][i]['visibility'] < MIN_VISIBILITY:
				continue
		
			divider = divider + 1
		
			avg_x = avg_x + SMOOTHING_FRAMES[j][i]['x']
			avg_y = avg_y + SMOOTHING_FRAMES[j][i]['y']
			avg_z = avg_z + SMOOTHING_FRAMES[j][i]['z']
			
			
		avg_x = avg_x / divider
		avg_y = avg_y / divider
		avg_z = avg_z / divider
		
		
		smooth_pos = {}
		smooth_pos['visibility'] = landmarks[i]['visibility']
		smooth_pos['x'] = avg_x
		smooth_pos['y'] = avg_y
		smooth_pos['z'] = avg_z
		
		result.append(smooth_pos)
		
	return result
	
	

def convert_mediapipe_position_data(mp_results, frame_depth):
	
	result = None
	
	if mp_results.pose_world_landmarks != None:
		
		landmarks = mp_results.pose_landmarks.landmark
		
		if landmarks[23].visibility < MIN_VISIBILITY or landmarks[24].visibility < MIN_VISIBILITY:
			return False, result 
			
		result = []
		
		for i in range(33):
			landmark = landmarks[i]
			
			pos_3d = {}
			
			if landmark.x < 0 or landmark.y < 0 or landmark.x >= 1.0 or landmark.y >= 1.0:
				pos_3d['visibility'] = 0.0
				result.append(pos_3d)
				continue
			
			real_x = math.floor(landmark.x * KINECT_IMAGE_SHAPE[0])
			real_y = math.floor(landmark.y * KINECT_IMAGE_SHAPE[1])
			
			pos_3d = {}
			pos_3d['x'] = frame_depth[real_y][real_x][0]
			pos_3d['y'] = -frame_depth[real_y][real_x][1]
			pos_3d['z'] = frame_depth[real_y][real_x][2]
			pos_3d['visibility'] = landmark.visibility
			

			if math.isinf(pos_3d['x']) or math.isinf(pos_3d['y']) or math.isinf(pos_3d['z']):
				pos_3d['visibility'] = 0.0
			
			result.append(pos_3d)
		
		if result[23]['visibility'] <= MIN_VISIBILITY or result[24]['visibility'] <= MIN_VISIBILITY:
			return False, result
				
		mid_depth_x = (result[23]['x'] + result[24]['x']) * 0.5
		mid_depth_y = (result[23]['y'] + result[24]['y']) * 0.5
		mid_depth_z = (result[23]['z'] + result[24]['z']) * 0.5
		
		for i in range(33):
			
			if result[i]['visibility'] <= MIN_VISIBILITY:
				continue
		
			result[i]['x'] = result[i]['x'] - mid_depth_x
			result[i]['y'] = result[i]['y'] - mid_depth_y
			result[i]['z'] = result[i]['z'] - mid_depth_z
			
			if abs(result[i]['z']) >= 0.75:
				result[i]['visibility'] = 0.0
		
		return True, result
				
	else:
		return False, result
	
	
	return False, result


def use_single_cam(cam_id):
	global RUNNING
	
	pose = mp_pose.Pose(min_detection_confidence=MIN_CONFIDENCE_FACTOR, min_tracking_confidence=MIN_TRACKING_CONFIDENCE, model_complexity=MODEL_COMPLEXITY)
	last_fps = 0
	
	
	while RUNNING:
	
		if cv2.waitKey(1) & 0xFF == 27:
			RUNNING = False
			break
	
		if not KINECT.has_new_depth_frame() or not KINECT.has_new_color_frame():
			continue 
		
		c_ticks = time.time()

		depthframeD = KINECT._depth_frame_data
		colourframe = KINECT.get_last_color_frame()
		
		colourframe = np.reshape(colourframe, (2073600, 4))
		colourframe = colourframe[:,0:3] 
		
		colourframeR = colourframe[:,0]
		colourframeR = np.reshape(colourframeR, (1080, 1920))
		colourframeG = colourframe[:,1]
		colourframeG = np.reshape(colourframeG, (1080, 1920))		 
		colourframeB = colourframe[:,2]
		colourframeB = np.reshape(colourframeB, (1080, 1920))
		framefullcolour = cv2.merge([colourframeR, colourframeG, colourframeB])

		results = pose.process(framefullcolour)
			   
		if results != None:
				   
			color2world_points_type = PyKinectV2._CameraSpacePoint * np.int(1920 * 1080)
			color2world_points = ctypes.cast(color2world_points_type(), ctypes.POINTER(PyKinectV2._CameraSpacePoint))
			
			error_state = KINECT._mapper.MapColorFrameToCameraSpace(ctypes.c_uint(512 * 424), depthframeD, ctypes.c_uint(1920 * 1080), color2world_points)
			
			if not error_state:
				
				
				pf_csps = ctypes.cast(color2world_points, ctypes.POINTER(ctypes.c_float))
				data = np.ctypeslib.as_array(pf_csps, shape=(1080, 1920, 3))

				is_ok, pose_results = convert_mediapipe_position_data(results, data)
				
				
				if is_ok == True:
					final_landmarks = smooth_landmarks(pose_results)
					packet_count = send_landmarks(final_landmarks, last_fps, 0)
					
				last_fps = 1.0 / (time.time() - c_ticks)
			
			
				if DRAW_DEBUG:
					mp_drawing.draw_landmarks(
						framefullcolour,
						results.pose_landmarks,
						mp_pose.POSE_CONNECTIONS,
						landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style())
				
					cv2.putText(framefullcolour,str(last_fps), (20,40), FONT, FONT_SCALE,FONT_COLOR,FONT_THICKNESS,FONT_LINETYPE)
					
			
			cv2.imshow('Mediapipe_Camera_Preview', framefullcolour)
			  
	KINECT.close()
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
