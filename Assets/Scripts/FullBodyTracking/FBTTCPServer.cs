using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;  

/// <summary>
/// Class that manages the tcp connection for the scripts to detect the quest etc.
/// </summary>
public class FBTTCPServer : MonoBehaviour 
{  	

	private TcpListener tcpListener; 
	
	private List<TcpClient> connected_clients;
	private Thread tcpListenerThread;  

	private String device_name;	
	
	
	/// <summary>
    /// Initialize the servet etc.
    /// </summary>
	void Start () 
    {
		DontDestroyOnLoad(this.gameObject);

		this.device_name = SystemInfo.deviceName;
		this.connected_clients = new List<TcpClient>(); 

		// Start TcpServer background thread 		
		this.tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		this.tcpListenerThread.IsBackground = true; 		
		this.tcpListenerThread.Start(); 	
	}  	
	
	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests () 
    { 		
		try 
        { 					
			tcpListener = new TcpListener(IPAddress.Any, 8081); 			
			tcpListener.Start();              
			Debug.Log("Server is listening");              

			while (true) 
            { 
                TcpClient client = tcpListener.AcceptTcpClient(); 	
                SendMessage(client, this.device_name);		

				this.connected_clients.Add(client);											
			} 		
		} 		
		catch (SocketException socketException) 
        { 			
			Debug.Log("SocketException " + socketException.ToString()); 		
		}     
	}  	

	/// <summary>
    /// Sends messages to the given client
    /// </summary>
    /// <param name="client">TcpClient the client we want to send a message to</param>
    /// <param name="message">string of the message we want to send</param>
	private void SendMessage(TcpClient client, String message) 
    { 		
		if (client == null) 
        {             
			return;         
		}  	

		Debug.Log("trying to send message to client");	
		
		try 
        { 			
			// Get a stream object for writing. 
			NetworkStream stream = client.GetStream(); 	
            		
			if (stream.CanWrite) 
            {                 	
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(message); 				
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);               
				Debug.Log("Server sent his message - should be received by client");           
			}       
		} 		
		catch (SocketException socketException) 
		{             
			Debug.Log("Socket exception: " + socketException);         
		} 	
	} 

	/// <summary>
    /// Stops the tcpListener thread from running when we get destroyed 
    /// </summary>
	public void OnDestroy ()
    {
        if(this.tcpListenerThread != null)
        {
            this.tcpListenerThread.Abort();
        }
    }
}