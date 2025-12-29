import numpy as np

import cv2
import subprocess
import tkinter as tk
import tkinter.ttk as ttk
from ttkwidgets import CheckboxTreeview
import re
import socket
from turtle import width

from tkinter import * 
from tkinter import filedialog
from tkinter import simpledialog

from MediaPipeUDPPoses import run_mediapipe_client
#from KinectUDPPose import run_kinect_client

def get_connection_device_ip():

	output = subprocess.check_output(("arp", "-a"))
	output = output.decode('ascii')
	all_ips = re.findall( r'[0-9]+(?:\.[0-9]+){3}', output )
	lan_ips = []
	usable_ips = []

	for ip in all_ips:

		if '192.168' in ip or '172.16' in ip or '10.' in ip: #check for all class c ips in our arp list
			lan_ips.append(ip)


	for ip in lan_ips:
		try:
			sock = socket.create_connection( (ip, 8081), timeout=2.0 )
			data = sock.recv(1024)
			
			if data == None:
				data = '<Unknown>'
			else:
				data = data.decode('utf-8')
			
			usable_ips.append([ip, data])
		except socket.error as e:
			print(ip, e)
			
	return usable_ips

	
def get_usable_webcams():
	dev_port = 0
	fail_ports = 0
	working_ports = []
	
	while fail_ports < 2: # if there are more than 2 non working ports stop the testing. 
		uses_dshow = True
		camera = cv2.VideoCapture(dev_port, cv2.CAP_DSHOW)
		if not camera.isOpened():
			camera = cv2.VideoCapture(dev_port)
			uses_dshow = False
			
		if camera.isOpened():
			fail_ports = 0
			is_reading, img = camera.read()
			w = camera.get(3)
			h = camera.get(4)
			
			if is_reading:
				print("Port %s is working and reads images (%s x %s)" %(dev_port,h,w))
				working_ports.append([dev_port, uses_dshow] )
		else:
			fail_ports += 1
			
		dev_port +=1
		
	return working_ports
	
class Application(tk.Frame):
	def __init__(self, root):
		self.usable_ips = None
		self.usable_cams = []
		self.connection_devices = []
		self.kinect_id = -1
		self.root = root
		self.initialize_user_interface()	

	def initialize_user_interface(self):
		self.root.title("Full Body Tracking")

		c1_pane = tk.Frame(self.root)
		c1_pane.grid(column=0, row=0, sticky="W")
		c2_pane = tk.Frame(self.root)
		c2_pane.grid(column=0, row=1, sticky="W")
		c3_pane = tk.Frame(self.root)
		c3_pane.grid(column=0, row=2, sticky="W")

		self.ip_lans = []
		self.scan_button = tk.Button(c1_pane, text="Scan Cams & IPs", command=self.scan, anchor=tk.N)
		self.scan_button.grid(column=2, row=0, sticky="W", padx=10, pady=10)

		self.connect_button = tk.Button(c1_pane, text="Connect with Mediapipe", command=self.connect_to)
		self.connect_button.grid(column=3, row=0, sticky="W", padx=10, pady=10)
		
		self.connect_button = tk.Button(c1_pane, text="Connect with Kinect", command=self.connect_with_kinect)
		self.connect_button.grid(column=4, row=0, sticky="W", padx=10, pady=10)

		#self.clear_button = tk.Button(c1_pane, text="Clear", command=self.clear_cams)
		#self.clear_button.grid(column=3, row=0, sticky="W", padx=5, pady=5)

		self.exit_button = tk.Button(c1_pane, text="Exit", command=self.root.destroy)
		self.exit_button.grid(column=7, row=0, sticky="W", padx=10, pady=10)

		#self.disconnect_button = tk.Button(c1_pane, text="Disconnect", command=self.switch)
		#self.disconnect_button.grid(column=5, row=0, sticky="W", padx=10, pady=10,)

		# Set the treeview
		self.ttk = ttk
		self.cam_tree = CheckboxTreeview(c2_pane, column=("column1", "column2", "column3"), show=("headings", "tree"))
		self.cam_tree.pack()
		self.ttk.Style().configure('Treeview', rowheight=20, width=20,height=20)
		self.cam_tree.heading("#0", text="Select")
		self.cam_tree.heading("#1", text="Camera")
		self.cam_tree.heading("#2", text="Dshow")
		
		self.ip_tree = CheckboxTreeview(c3_pane, column=("column1", "column2", "column3"), show=("headings", "tree"))
		self.ip_tree.pack()
		self.ttk.Style().configure('Treeview', rowheight=20, width=100,height=20)
		self.ip_tree.heading("#0", text="Select")
		self.ip_tree.heading("#1", text="Host Name")
		self.ip_tree.heading("#2", text="IP Address")
		
		style = ttk.Style()
		style.theme_use("clam")
		#self.tree.heading("#2", text="IP")
		self.treeview = self.cam_tree
		self.iid = 1

	def clear_cams(self):
		self.cam_tree.delete(*self.cam_tree.get_children())
		
	def clear_ips(self):
		self.ip_tree.delete(*self.ip_tree.get_children())

	def scan(self):
		self.clear_cams()
		self.clear_ips()
		# ip_lans = test_scan_devices()
		self.usable_cams = get_usable_webcams()
		self.usable_ips = get_connection_device_ip()
		
		id = 0
		
		for cam in self.usable_cams:
			self.cam_tree.insert('', 'end', iid=id, values=(f"Cam_{cam[0]}", cam[1]))
			id = id + 1
			
		id = 0
		
		for ip in self.usable_ips:
			self.ip_tree.insert('', 'end', iid=id, values=(f"{ip[1]}", ip[0]))
			id = id + 1

	def get_checked(self, treeview):
		checked = []

		def rec_get_checked(item):
			if treeview.tag_has('checked', item):
				checked.append(item)
			for ch in treeview.get_children(item):
				rec_get_checked(ch)

		rec_get_checked('')
		
		return checked

	def connect_with_kinect(self):
	
		if self.usable_ips == None or len(self.usable_ips) <= 0:
			return 
			
		wanted_ip = self.get_checked(self.ip_tree)
		
		if wanted_ip == None or len(wanted_ip) <= 0:
			return 
			
		target_ip = self.usable_ips[ int(wanted_ip[0]) ]
		run_kinect_client(target_ip[0])

	def connect_to(self):
	
		if self.usable_cams == None or len(self.usable_cams) <= 0 or self.usable_ips == None or len(self.usable_ips) <= 0:
			return 
	
		wanted_cams = self.get_checked(self.cam_tree)
		wanted_ip = self.get_checked(self.ip_tree)
		
		if wanted_ip == None or len(wanted_ip) <= 0 or wanted_cams == None or len(wanted_cams) <= 0:
			return 
			
		target_cams = []
		target_cams_dshow = []
		target_ip = self.usable_ips[ int(wanted_ip[0]) ]
		
		for i in range(len(wanted_cams)):
			target_cams.append(self.usable_cams[ int(wanted_cams[i]) ][0] )
			target_cams_dshow.append(self.usable_cams[ int(wanted_cams[i]) ][1] )
		
		
		run_mediapipe_client(target_ip[0], target_cams, target_cams_dshow)

app = Application(tk.Tk())
app.root.mainloop()