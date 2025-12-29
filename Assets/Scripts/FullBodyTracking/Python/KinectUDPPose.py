from pykinect2 import PyKinectV2
from pykinect2.PyKinectV2 import *
from pykinect2 import PyKinectRuntime

import ctypes
import _ctypes
import pygame
import sys

import time
import math

import socket
import struct
import os

import numpy as np

import subprocess
import socket

import transformations


UDP_IP = "127.0.0.1"
UDP_PORT = 8081
GAME = None

SOCK = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
SKELETON_COLOR = pygame.color.THECOLORS["red"]

class PoseDetection(object):
    def __init__(self):
        pygame.init()

        # Used to manage how fast the screen updates
        self._clock = pygame.time.Clock()

        # Set the width and height of the screen [width, height]
        self._infoObject = pygame.display.Info()
        self._screen = pygame.display.set_mode((self._infoObject.current_w >> 1, self._infoObject.current_h >> 1), 
                                               pygame.HWSURFACE|pygame.DOUBLEBUF|pygame.RESIZABLE, 32)

        pygame.display.set_caption("Kinect camera pose")

        # Loop until the user clicks the close button.
        self._done = False

        # Used to manage how fast the screen updates
        self._clock = pygame.time.Clock()
        self.c_ticks = time.time()

        # Kinect runtime object, we want only color and body frames 
        self._kinect = PyKinectRuntime.PyKinectRuntime(PyKinectV2.FrameSourceTypes_Color | PyKinectV2.FrameSourceTypes_Body)

        # back buffer surface for getting Kinect color frames, 32bit color, width and height equal to the Kinect color frame size
        self._frame_surface = pygame.Surface((self._kinect.color_frame_desc.Width, self._kinect.color_frame_desc.Height), 0, 32)

        # here we will store skeleton data 
        self._bodies = None


    def draw_body_bone(self, joints, jointPoints, color, joint0, joint1):
        joint0State = joints[joint0].TrackingState;
        joint1State = joints[joint1].TrackingState;

        # both joints are not tracked
        if (joint0State == PyKinectV2.TrackingState_NotTracked) or (joint1State == PyKinectV2.TrackingState_NotTracked): 
            return

        # both joints are not *really* tracked
        if (joint0State == PyKinectV2.TrackingState_Inferred) and (joint1State == PyKinectV2.TrackingState_Inferred):
            return

        # ok, at least one is good 
        start = (jointPoints[joint0].x, jointPoints[joint0].y)
        end = (jointPoints[joint1].x, jointPoints[joint1].y)

        try:
            pygame.draw.line(self._frame_surface, color, start, end, 8)
        except: # need to catch it due to possible invalid positions (with inf)
            pass
            
    def send_landmarks(self, bone_results, fps, cam_id):
    
        if bone_results == None or len(bone_results) <= 0:
            return 
    
        packet_count = 0
        
        struct_datatype = "bddd"
        packed_entries = []
        
        for i in range(len(bone_results)):
        
            landmark = bone_results[i]
            
            if landmark["visibility"] > 0.5:
                packed_entries.append(i)
                packed_entries.append(-landmark["x"])
                packed_entries.append(-landmark["y"])
                packed_entries.append(landmark["z"])
                packet_count = packet_count + 1

        
        if packet_count > 0:
            timestamp = time.time()
        
            struct_datatype = "<" + "d" + "d" + "d" + "d" + struct_datatype*packet_count
            udp_data_array = bytearray(struct.pack(struct_datatype, cam_id, 1, timestamp,fps, *packed_entries))
            
            SOCK.sendto(udp_data_array , (UDP_IP, UDP_PORT))
            
            
        return packet_count

    def draw_body(self, joints, jointPoints, color):
        # Torso
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_Head, PyKinectV2.JointType_Neck);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_Neck, PyKinectV2.JointType_SpineShoulder);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineShoulder, PyKinectV2.JointType_SpineMid);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineMid, PyKinectV2.JointType_SpineBase);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineShoulder, PyKinectV2.JointType_ShoulderRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineShoulder, PyKinectV2.JointType_ShoulderLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineBase, PyKinectV2.JointType_HipRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_SpineBase, PyKinectV2.JointType_HipLeft);
    
        # Right Arm    
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_ShoulderRight, PyKinectV2.JointType_ElbowRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_ElbowRight, PyKinectV2.JointType_WristRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_WristRight, PyKinectV2.JointType_HandRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_HandRight, PyKinectV2.JointType_HandTipRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_WristRight, PyKinectV2.JointType_ThumbRight);

        # Left Arm
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_ShoulderLeft, PyKinectV2.JointType_ElbowLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_ElbowLeft, PyKinectV2.JointType_WristLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_WristLeft, PyKinectV2.JointType_HandLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_HandLeft, PyKinectV2.JointType_HandTipLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_WristLeft, PyKinectV2.JointType_ThumbLeft);

        # Right Leg
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_HipRight, PyKinectV2.JointType_KneeRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_KneeRight, PyKinectV2.JointType_AnkleRight);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_AnkleRight, PyKinectV2.JointType_FootRight);

        # Left Leg
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_HipLeft, PyKinectV2.JointType_KneeLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_KneeLeft, PyKinectV2.JointType_AnkleLeft);
        self.draw_body_bone(joints, jointPoints, color, PyKinectV2.JointType_AnkleLeft, PyKinectV2.JointType_FootLeft);
        
        
    def add_bone_to_list(self, array, idx, joint_array, joint):
    
    
        joint_state = joint_array[joint].TrackingState
        is_visible = True
        
        if joint_state == PyKinectV2.TrackingState_NotTracked:
            is_visible = False
        
        if joint_state == PyKinectV2.TrackingState_Inferred:
            is_visible = False   

        if is_visible == True:
            array[idx]["visibility"] = 1.0
            array[idx]["x"] = joint_array[joint].Position.x
            array[idx]["y"] = joint_array[joint].Position.y
            array[idx]["z"] = joint_array[joint].Position.z
            
    def add_raw_bone_to_list(self, array, idx, x,y,z):
        array[idx]["visibility"] = 1.0
        array[idx]["x"] = x
        array[idx]["y"] = y
        array[idx]["z"] = z
        
    def convert_to_mediapipe_bones(self, body, joints, jointPoints):
    
        result_array = []
        
        for i in range(33):
            entry = {}
            entry["visibility"] = 0.0
            entry["x"] = 0.0
            entry["y"] = 0.0
            entry["z"] = 0.0
            
            result_array.append(entry)

        #left arm
        self.add_bone_to_list(result_array, 11, joints, PyKinectV2.JointType_ShoulderLeft)
        self.add_bone_to_list(result_array, 13, joints, PyKinectV2.JointType_ElbowLeft )
        self.add_bone_to_list(result_array, 15, joints, PyKinectV2.JointType_WristLeft )
        self.add_bone_to_list(result_array, 21, joints, PyKinectV2.JointType_ThumbLeft )
        self.add_bone_to_list(result_array, 19, joints, PyKinectV2.JointType_HandTipLeft )
        self.add_bone_to_list(result_array, 17, joints, PyKinectV2.JointType_HandTipLeft )#its ok to use the same, here since it wont matter for our calculations
        
        #right arm
        self.add_bone_to_list(result_array, 12, joints, PyKinectV2.JointType_ShoulderRight)
        self.add_bone_to_list(result_array, 14, joints, PyKinectV2.JointType_ElbowRight )
        self.add_bone_to_list(result_array, 16, joints, PyKinectV2.JointType_WristRight )
        self.add_bone_to_list(result_array, 22, joints, PyKinectV2.JointType_ThumbRight )
        self.add_bone_to_list(result_array, 20, joints, PyKinectV2.JointType_HandTipRight )
        self.add_bone_to_list(result_array, 18, joints, PyKinectV2.JointType_HandTipRight )#its ok to use the same, here since it wont matter for our calculations
        
        #left leg
        self.add_bone_to_list(result_array, 23, joints, PyKinectV2.JointType_HipLeft)
        self.add_bone_to_list(result_array, 25, joints, PyKinectV2.JointType_KneeLeft)
        self.add_bone_to_list(result_array, 27, joints, PyKinectV2.JointType_AnkleLeft)
        self.add_bone_to_list(result_array, 29, joints, PyKinectV2.JointType_AnkleLeft)
        self.add_bone_to_list(result_array, 31, joints, PyKinectV2.JointType_FootLeft)
        
        #right leg
        self.add_bone_to_list(result_array, 24, joints, PyKinectV2.JointType_HipRight)
        self.add_bone_to_list(result_array, 26, joints, PyKinectV2.JointType_KneeRight)
        self.add_bone_to_list(result_array, 28, joints, PyKinectV2.JointType_AnkleRight)
        self.add_bone_to_list(result_array, 30, joints, PyKinectV2.JointType_AnkleRight)
        self.add_bone_to_list(result_array, 32, joints, PyKinectV2.JointType_FootRight)
        
        #head (note that the head itself is special since we only have one point here)
        joint_state = joints[PyKinectV2.JointType_Neck].TrackingState

        if joint_state != PyKinectV2.TrackingState_NotTracked:
            if joint_state != PyKinectV2.TrackingState_Inferred:
            
                tx = joints[PyKinectV2.JointType_Neck].Position.x
                ty = joints[PyKinectV2.JointType_Neck].Position.y
                tz = joints[PyKinectV2.JointType_Neck].Position.z
            
                qx = body.joint_orientations[PyKinectV2.JointType_Neck].Orientation.x
                qy = body.joint_orientations[PyKinectV2.JointType_Neck].Orientation.y
                qz = body.joint_orientations[PyKinectV2.JointType_Neck].Orientation.z
                qw = body.joint_orientations[PyKinectV2.JointType_Neck].Orientation.w
                
                rot_matrix = transformations.quaternion_matrix([qw,qx,qy,qz])
                scl = 0.1
                
                left_eye_x = tx + rot_matrix[0][0] * scl
                left_eye_y = ty + rot_matrix[1][0] * scl
                left_eye_z = tz + rot_matrix[2][0] * scl
                
                right_eye_x = tx - rot_matrix[0][0] * scl
                right_eye_y = ty - rot_matrix[1][0] * scl
                right_eye_z = tz - rot_matrix[2][0] * scl
                
                left_ear_x = left_eye_x - rot_matrix[0][2] * scl
                left_ear_y = left_eye_y - rot_matrix[1][2] * scl
                left_ear_z = left_eye_z - rot_matrix[2][2] * scl
                
                right_ear_x = right_eye_x - rot_matrix[0][2] * scl
                right_ear_y = right_eye_y - rot_matrix[1][2] * scl
                right_ear_z = right_eye_z - rot_matrix[2][2] * scl
                
                self.add_raw_bone_to_list(result_array, 2, left_eye_x, left_eye_y, left_eye_z)
                self.add_raw_bone_to_list(result_array, 5, right_eye_x, right_eye_y, right_eye_z)
                
                self.add_raw_bone_to_list(result_array, 7, left_ear_x, left_ear_y, left_ear_z)
                self.add_raw_bone_to_list(result_array, 8, right_ear_x, right_ear_y, right_ear_z)
                
        joint_state = joints[PyKinectV2.JointType_SpineBase].TrackingState

        if joint_state != PyKinectV2.TrackingState_NotTracked:
            if joint_state != PyKinectV2.TrackingState_Inferred:
                
                base_x = joints[PyKinectV2.JointType_SpineBase].Position.x
                base_y = joints[PyKinectV2.JointType_SpineBase].Position.y
                base_z = joints[PyKinectV2.JointType_SpineBase].Position.z
                
                for i in range(len(result_array)):
                    result_array[i]['x'] = result_array[i]['x'] - base_x
                    result_array[i]['y'] = result_array[i]['y'] - base_y
                    result_array[i]['z'] = result_array[i]['z'] - base_z                    

        return result_array


    def draw_color_frame(self, frame, target_surface):
        target_surface.lock()
        address = self._kinect.surface_as_array(target_surface.get_buffer())
        ctypes.memmove(address, frame.ctypes.data, frame.size)
        del address
        target_surface.unlock()

    def run(self):
        # -------- Main Program Loop -----------
        while not self._done:
            # --- Main event loop
            for event in pygame.event.get(): # User did something
                if event.type == pygame.QUIT: # If user clicked close
                    self._done = True # Flag that we are done so we exit this loop

                elif event.type == pygame.VIDEORESIZE: # window resized
                    self._screen = pygame.display.set_mode(event.dict['size'], 
                                               pygame.HWSURFACE|pygame.DOUBLEBUF|pygame.RESIZABLE, 32)
                    
            # --- Game logic should go here

            # --- Getting frames and drawing  
            # --- Woohoo! We've got a color frame! Let's fill out back buffer surface with frame's data 
            

            if self._kinect.has_new_color_frame():
                frame = self._kinect.get_last_color_frame()
                self.draw_color_frame(frame, self._frame_surface)
                frame = None

            # --- Cool! We have a body frame, so can get skeletons
            if self._kinect.has_new_body_frame(): 
            
                fps = 1.0 / (time.time() - self.c_ticks)
                self.c_ticks = time.time()
                
                print(fps)
            
                self._bodies = self._kinect.get_last_body_frame()
            else:
                self._bodies = None

            # --- draw skeletons to _frame_surface
            if self._bodies is not None: 
                for i in range(0, self._kinect.max_body_count):
                    body = self._bodies.bodies[i]
                    if not body.is_tracked: 
                        continue 
                    
                    joints = body.joints 
                    # convert joint coordinates to color space 
                    joint_points = self._kinect.body_joints_to_color_space(joints)
                    self.draw_body(joints, joint_points, SKELETON_COLOR)
                    
                    mediapipe_joints = self.convert_to_mediapipe_bones(body, joints, joint_points)
                    self.send_landmarks(mediapipe_joints, fps, 0)
                    
                    break #we dont want to detect multiple person

            # --- copy back buffer surface pixels to the screen, resize it if needed and keep aspect ratio
            # --- (screen size may be different from Kinect's color frame size) 
            h_to_w = float(self._frame_surface.get_height()) / self._frame_surface.get_width()
            target_height = int(h_to_w * self._screen.get_width())
            surface_to_draw = pygame.transform.scale(self._frame_surface, (self._screen.get_width(), target_height));
            self._screen.blit(surface_to_draw, (0,0))
            surface_to_draw = None
            pygame.display.update()

            # --- Go ahead and update the screen with what we've drawn.
            pygame.display.flip()

            # --- Limit to 60 frames per second
            self._clock.tick(60)

        # Close our Kinect sensor, close the window and quit.
        self._kinect.close()
        pygame.quit()


def run_kinect_client(ip):
    global GAME, UDP_IP
    
    print("Starting pose detection with the following parameter")
    print(ip)
    print("kinect camera")
    print("--------------")

    UDP_IP = ip
    
    GAME = PoseDetection()
    GAME.run()


def close_kinect_client():
    GAME._done = True


if __name__ == '__main__':  
    run_kinect_client("127.0.0.1")

