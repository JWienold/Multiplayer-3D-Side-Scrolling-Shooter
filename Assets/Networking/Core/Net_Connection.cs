using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Networking {
	public class Net_Connection
	{

		public Socket connection;
		public int playerId;
		public byte[] buffer = new byte[1024];
		public StringBuilder sb = new StringBuilder();
		public bool didRecieve = false;
		public Net_Core myCore;
		public bool isSending = false;
		public object _sendLock = new object();
		public bool isDisconnecting = false;
		public bool didDisc = false;
		public bool kicky = false;
		// Use this for initialization
		void Start()
		{

		}

		/// <summary>
		/// SEND STUFF
		/// This will deal with sending information across the current 
		/// NET_Connection's socket.
		/// </summary>
		/// <param name="byteData"></param>
		public void Send(byte[] byteData)
		{

				connection.BeginSend(byteData, 0, byteData.Length, 0,
					new AsyncCallback(this.SendCallback), connection);
			isSending = true;
		}

		private void SendCallback(IAsyncResult ar)
		{
			try
			{
				isSending = false;
				// Retrieve the socket from the state object.  
				//Socket handler = (Socket)ar.AsyncState;
				// Complete sending the data to the remote device.  
				//int bytesSent = handler.EndSend(ar);	  
				if(isDisconnecting && myCore.isClient)
				{

					didDisc = true;
				}	  
			}
			catch (Exception)
			{
				Debug.Log("Problem???");
			}
		}

		public IEnumerator Recieve()
		{
			while (true)
			{
				try
				{
					connection.BeginReceive(buffer, 0, 1024, 0, 
						new AsyncCallback(ReceiveCallback), this);
				}
				catch (ObjectDisposedException)
				{   //This should only happen when the socket died
					//so we are going to diconnect it.
					myCore.Disconnect(this);
				}
				yield return new WaitUntil(() => didRecieve);
				//We must process information here because we cannot create on a call back.
				
				string responce = sb.ToString();
				if (responce.Trim(' ')=="")
				{	  
					//We do NOT want any empty strings.  It will cause a problem.
					myCore.Disconnect(this);
				}
				//Parse string
				string[] commands = responce.Split('\n');
				for (int i = 0; i < commands.Length; i++)
				{
					if (commands[i].Trim(' ') == "")
					{  
						continue;
					}
					if (commands[i].Trim(' ') == "OK" && myCore.isClient)
					{
						Debug.Log("Client Recieved OK.");
						//Do nothing, just a heartbeat
					}
					else if (commands[i].StartsWith("PlayerID") && myCore.isClient)
					{
						try
						{
							//This is how the client get's their player id.
							playerId = int.Parse(commands[i].Split('_')[1]);
							myCore.localPlayerId = playerId;
						}
						catch (FormatException)
						{
							Debug.Log("Got scrambled Message: " + commands[i]);
						}
					}
					else if (commands[i].StartsWith("DIRTY") && myCore.isServer)
					{
						int currentNum =0;
						try
						{
							string[] args = commands[i].Split('_');
							currentNum = int.Parse(args[1]);
							Net_Component[] AllScripts =
							myCore.netObjs[currentNum].obj.GetComponents<Net_Component>();
							for (int j = 0; j < AllScripts.Length; j++)
							{
								AllScripts[j].isDirty = true;
							}
						}
						catch(System.Collections.Generic.KeyNotFoundException)
						{
							myCore.NetDestroyObject(currentNum);
							//This means the object was destroyed on the server but the client still requested an update.
							//Should not cause any problem.  By the time the server sees this.  The client should have 
							//Destroyed the object.
						}
						catch (FormatException)
						{
							Debug.Log("Got scrambled Message: " + commands[i]);
						}
					}
					else if (commands[i].StartsWith("CREATE") && myCore.isClient)
					{
						//CREATE_<TYPE>_<OWNER>_<NETID>
						try
						{
							string[] args = commands[i].Split('_');
							int type = int.Parse(args[1]);
							int owner = int.Parse(args[2]);
							int netId = int.Parse(args[3]);
							Vector3 temp = Vector3.zero;
							if (args.Length >= 7)
							{
							 temp = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6])); 
							}
							GameObject np = GameObject.Instantiate(myCore.spawnPrefabs[type],temp,Quaternion.identity);

							np.GetComponent<Net_ID>().owner = owner;
							np.GetComponent<Net_ID>().netId = netId;
							myCore.netObjs.Add(netId,new Net_Objects(np, type, owner, netId));
							np.GetComponent<Net_ID>().notifyDirty();
						}
						catch (FormatException)
						{
							Debug.Log("Got scrambled Message: " + commands[i]);
						}
						catch (Exception e)
						{
							Debug.Log("Exception Occurred in object spawning - " + e.Message);

						}
					}
					else if(commands[i].StartsWith("DELETE_") && myCore.isClient)
					{
						try
						{
							int badId = int.Parse(commands[i].Split('_')[1]);
							GameObject.Destroy(myCore.netObjs[badId].obj);
							myCore.netObjs.Remove(badId);
						}
						catch (FormatException)
						{
							Debug.Log("Got scrambled Message: " + commands[i]);
						}
						catch
						{
							//We already destroyed it and this is a repeat.
						}
					}
					else if(commands[i].StartsWith("DISCON_") && myCore.isServer)
					{
						try
						{
							int badCon = int.Parse(commands[i].Split('_')[1]);
							myCore.Disconnect(myCore.connections[badCon]);
							Debug.Log("There are now only " + myCore.connections.Count + " players in the game.");
						}
						catch(FormatException)
						{
							Debug.Log("We received a scrambled message+ " + commands[i]);
						}
						catch(Exception e)
						{
							Debug.Log("Unkown exception: " + e.ToString());
						}
					}
					else if(commands[i].StartsWith("DISCON_") && myCore.isClient)
					{
						kicky = true;
						myCore.LeaveGame();
					}
					else
					{
						//Assume we are updating an object... we will do this later.  
						myCore.UpdateNetObjects(commands[i]);
					}
				}

				sb.Length = 0;
				sb = new StringBuilder();
				didRecieve = false;
				yield return new WaitForSeconds(.05f);
			}//closes while(true)			
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				Net_Connection temp = (Net_Connection)ar.AsyncState;
				int bytesRead = -1;			 
				bytesRead = connection.EndReceive(ar);		   
				if (bytesRead > 0)
				{
					temp.sb.Append(Encoding.ASCII.GetString(temp.buffer, 0,
						bytesRead));
					string ts = temp.sb.ToString();
					if(ts[ts.Length-1]!='\n')
					{
					 connection.BeginReceive(buffer, 0, 1024, 0,new AsyncCallback(ReceiveCallback), this);
					}
					else
					{
						temp.didRecieve = true;
					}
				}
			}
			catch (Exception)
			{
				//We have to disconnect at the main thread not on the 
				//call back.
			}
		}
	}
}
