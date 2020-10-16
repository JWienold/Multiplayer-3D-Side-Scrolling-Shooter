using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Networking {
	public struct Net_Objects
	{
		public GameObject obj;
		public int type;
		public int owner;
		public int netId;

		public Net_Objects(GameObject np, int TYPE, int OWNER, int NETID) : this()
		{
			this.obj = np;
			this.type = TYPE;
			this.owner = OWNER;
			this.netId = NETID;
		}
	}

	public class Net_Core : MonoBehaviour
	{
		//List of active connections(players) in the game
		public Dictionary<int, Net_Connection> connections;
		//A list of all network spawned objects
		public Dictionary<int, Net_Objects> netObjs;
		//Array of all prefabs that can be spawned 
		//over the network
		public GameObject[] spawnPrefabs;
		//Property: how many can connect
		public int maxConnections;
		//What is the ipAddress
		public string ipAddress;
		//What is the Port
		public int port;
		//Is this a server?
		public bool isServer;
		//Is this a client?
		public bool isClient;
		//Have we connected
		public bool isConnected;
		//What is the local players ID?
		public int localPlayerId;
		//Are we currently connecting someone (or are connecting)
		public bool currentlyConnecting;
		//this string will store all of messages form all synched network objects.
		public string masterMessage;
		// Use this for initialization
		public bool acceptMorePlayers = true;
		//This will count the current index for the connection
		public int conCounter = 0;
		//This will count the current object index
		public int objCounter = 0;
		public Socket listener;

		private object _conLock = new object();
		private object _objLock = new object();
		private object _masterMsg = new object();
		void Start()
		{
			/*
		   string[] args = System.Environment.GetCommandLineArgs();
			for(int i =0; i < args.Length; i ++)
			{

				if(args[i] == "server" || args[i] == "-S")
				{
					System.Diagnostics.Process Room2 = new Process();
					Room2.StartInfo.FileName = "demo.exe";
					Room2.StartInfo.Arguments = "-p 9001 -L2";
					Room2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					Room2.Start();
					
					Process Room3 = new Process();
					Room3.StartInfo.FileName = "demo.exe";
					Room3.StartInfo.Arguments = "-p 9002 -L3";
					Room3.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					Room3.Start();

					StartServer();
					break;
				}
				if(args[i] == "-p")
				{
					try
					{
						port = int.Parse(args[i + 1]);
					}
					catch
					{
						System.Environment.Exit(System.Environment.ExitCode);
					}
				}
				if(args[i] == "-L2")
				{
					//Load level 2 scene
					StartServer();
				}
				if(args[i] == "-L3")
				{
					//Load level 3 scene
					StartServer();
				}
			}


			if(isConnected == false)
			{
				StartClient();
			}*/

			isServer = false;
			isClient = false;
			isConnected = false;
			currentlyConnecting = false;
			//ipAddress = "127.0.0.1";//Local host
			if (ipAddress == "")
			{
				ipAddress = "127.0.0.1";//Local host
			}
			if (port == 0)
			{
				port = 9001;
			}
			connections = new Dictionary<int, Net_Connection>();
			netObjs = new Dictionary<int, Net_Objects>();

/*#if UNITY_STANDALONE_LINUX
			StartClient();
			//StartServer();
#else
			//Set the IP address for the specific 
			//dedicated server
			//StartClient();
			StartServer();
#endif*/
		 


		}
		///
		/// Server Functions
		///
		public void StartServer()
		{
			if (!isConnected)
			{
				isServer = false;
				isClient = false;
				isConnected = false;
				currentlyConnecting = false;
				StartCoroutine(Listen());
				StartCoroutine(slowUpdate());
				//Disable the UI.
				this.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
				this.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
			}
		}

		public IEnumerator Listen()
		{
			//If we are listening then we are the server.
			isServer = true;
			isConnected = true;
			isClient = false;
			localPlayerId = -1; //For server the localplayer id will be -1.
			//Initialize port to listen to
			
			IPAddress ip = (IPAddress.Any);
			IPEndPoint endP = new IPEndPoint(ip, port);
			//We could do UDP in some cases but for now we will do TCP
			listener = new Socket(ip.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);

			//Now I have a socket listener.
			listener.Bind(endP);
			listener.Listen(maxConnections);
			Debug.Log("We are now listening");
			while (acceptMorePlayers)
			{
				listener.BeginAccept
					(new AsyncCallback(this.AcceptCallBack), listener);
				yield return new WaitUntil(() => currentlyConnecting ||
				!acceptMorePlayers);

				if(connections.ContainsKey(conCounter -1)== false)
				{
					continue;
				}
				lock (_masterMsg)
				{
					//We tell the client what the Player ID is.
					connections[conCounter - 1].Send(Encoding.ASCII.GetBytes(
						"PlayerID_" + connections[conCounter - 1].playerId + "\n"));
					//Start Server side listening for client messages.
					StartCoroutine(connections[conCounter - 1].Recieve());
					currentlyConnecting = false;

					//Ignore this until we get to game objects.... 
					//Spawn ALL existing network Objects
					foreach (KeyValuePair<int, Net_Objects> entry in netObjs)
					{
						connections[conCounter - 1].Send(
							Encoding.ASCII.GetBytes("CREATE_" + entry.Value.type +
							"_" + entry.Value.owner + "_" + entry.Value.netId + "\n")
							);
						entry.Value.obj.GetComponent<Net_Component>().isDirty = true;
					}
					//End of Spawn All existing Network Objects
				}
				//Create Network Player Object - Let's just assume the first prefab is network player.
				NetCreateObject(0, conCounter - 1, Vector3.zero);
			}
		}
		//This will create the Connection on the server.
		public void AcceptCallBack(IAsyncResult ar)
		{
			Socket listener = (Socket)ar.AsyncState;
			Socket handler = listener.EndAccept(ar);
			Net_Connection temp = new Net_Connection();
			temp.connection = handler;
			temp.playerId = conCounter;
			conCounter++;
			temp.myCore = this;
			lock (_conLock)
			{
				connections.Add(temp.playerId, temp);
				Debug.Log("There are now " + connections.Count +
					" player(s) connected.");
			}
			currentlyConnecting = true;
		}

		///
		///Client Functions
		///
		public void StartClient()
		{
			if (!isConnected)
			{
				isServer = false;
				isClient = false;
				isConnected = false;
				currentlyConnecting = false;
				StartCoroutine(connectClient());
				this.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
				this.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
			}
		}
		public IEnumerator connectClient()
		{
			isClient = true;
			isServer = false;
			//Setup our socket
			IPAddress ip = (IPAddress.Parse(ipAddress));
			IPEndPoint endP = new IPEndPoint(ip, port);
			Socket clientSocket = new Socket(ip.AddressFamily, SocketType.Stream,
				ProtocolType.Tcp);
			//Connect client
			clientSocket.BeginConnect(endP, OnConnected, clientSocket);
			Debug.Log("Trying to wait for server...");
			//Wait for the client to connect
			yield return new WaitUntil(() => currentlyConnecting);
			StartCoroutine(connections[0].Recieve());  //It is 0 on the client because we only have 1 socket.
			StartCoroutine(slowUpdate());  //This will allow the client to send messages to the server.
		}

		public void OnConnected(IAsyncResult ar)
		{
			//Client will use the con list (but only have one entry).
			Net_Connection temp = new Net_Connection();
			temp.connection = (Socket)ar.AsyncState;
			temp.connection.EndConnect(ar);//This finishes the TCP connection (DOES NOT DISCONNECT)
			currentlyConnecting = true;
			isConnected = true;
			temp.myCore = this;
			connections.Add(0, temp);
		}







		//////////////
		//this is for the slow update
		/////////////

		public IEnumerator slowUpdate()
		{
			while (true)
			{
				//Find all of the network objects
				Net_ID[] networkObjs = GameObject.FindObjectsOfType<Net_ID>();
				//Get their current messages (This will work on the server or the client
				for (int i = 0; i < networkObjs.Length; i++)
				{
					lock (_masterMsg)
					{
						//Add their message to the masterMessage (the one we send)
						lock (networkObjs[i]._lock)
						{
							masterMessage += networkObjs[i].gameObjectMessages + "\n";
							//Clear Game Objects messages.
							networkObjs[i].gameObjectMessages = "";
						 }
					  
					}
				}
				//IF master message is not empty send message to all connections
				//This will only be one for the client
				//This will be all for the server.
				lock (_masterMsg)
				{   
					if (masterMessage != "")
					{   
						
						List<int> badCon = new List<int>();
						//yield return new WaitWhile(() => connections[entry.Key].isSending);
						try
						{
						lock (_conLock)
						{ 
							foreach (KeyValuePair<int, Net_Connection> entry in connections)
							{

									try
									{	  
										//For some reason the client is not throwing an exception						 
										connections[entry.Key].Send(Encoding.ASCII.
										GetBytes(masterMessage));
									}

									catch (Exception)
									{
										badCon.Add(entry.Key);
									}
								}

								for (int i = 0; i < badCon.Count; i++)
								{
									//Client has disconnected.
									if (connections.ContainsKey(badCon[i]))
									{
										Disconnect(connections[badCon[i]]);
									}
								}
								masterMessage = "";
							}
						}
						catch (Exception e)
						{
							Debug.Log("This is happening still! "+e.ToString());
						}
					}
				}
				yield return new WaitForSeconds(.05f);
			}
		}

		public void UpdateNetObjects(string msg)
		{
			string[] commands = msg.Split('_');
			try {
				if (netObjs.ContainsKey(int.Parse(commands[1])))
				{
					netObjs[int.Parse(commands[1])].obj.GetComponent<Net_ID>()
						.Net_Update(
						commands[0], commands[2], commands[3]);
				}
			}
			catch (Exception) {}
		}

		public void Disconnect(Net_Connection bad)
		{
			if (isClient)
			{
				bad.connection.Shutdown(SocketShutdown.Both);
				//but for now we will close it.
				bad.connection.Close();
				this.isClient = false;
				this.isServer = false;
				this.isConnected = false;
				this.localPlayerId = -10;
				foreach (KeyValuePair<int, Net_Objects> obj in netObjs)
				{
					Destroy(obj.Value.obj);
				}
				netObjs.Clear();
				connections.Clear();
			}
			if (isServer)
			{
  
					try
					{
						bad.connection.Shutdown(SocketShutdown.Both);
						bad.connection.Close();
					}
					catch (System.Net.Sockets.SocketException)
					{
						Debug.Log("Connection " + bad.playerId + " is already Closed!  Removing Objects.");
					}
					catch (ObjectDisposedException)
					{
						Debug.Log("Socket already shutdown: ");
					}
					catch
					{
					//In case anything else goes wrong.
					Debug.Log("Warning - Error caught in the generic catch!");
					}
					//Delete All other players objects....

					OnClientDisc(bad);
			}

		}
		//We can override this function for different functionality.
		//This is called automatically by the server.
		public virtual void OnClientDisc(Net_Connection bad)
		{
			if (isServer)
			{
				//Remove Connection from server
				connections.Remove(bad.playerId);
				List<int> badObjs = new List<int>();
				foreach (KeyValuePair<int, Net_Objects> obj in netObjs)
				{
					if (obj.Value.owner == bad.playerId)
					{
							badObjs.Add(obj.Key);
						//I have to add the key to a temp list and delete
						//it outside of this for loop
					}
				}
				//Now I can remove the netObjs from the dictionary.
				for (int i = 0; i < badObjs.Count; i++)
				{
					NetDestroyObject(badObjs[i]);
				}
			}
		}

		public void LeaveGame()
		{
			if (isClient && isConnected)
			{
				try
				{
					lock (_conLock)
					{
						Debug.Log("Sending Disconnect!");
					   connections[0].Send(Encoding.ASCII.
										GetBytes(
										"DISCON_" + connections[0].playerId.ToString() + "\n")
										);
						connections[0].isDisconnecting = true;
					}

						
				}
				catch(NullReferenceException)
				{
					//Client double-tapped disconnect.
					//Ignore.
				}
				StartCoroutine(WaitForDisc());
			}
			if (isServer && isConnected)
			{
				isServer = false;
				try
				{
					foreach (KeyValuePair<int, Net_Objects> obj in netObjs)
					{
						Destroy(obj.Value.obj);
					}
				}
				catch(NullReferenceException)
				{
					//Objects already destroyed.
				}
				try
				{
					foreach (KeyValuePair<int, Net_Connection> entry in connections)
					{
						entry.Value.Send(Encoding.ASCII.GetBytes("DISCON_\n"));
						Disconnect(entry.Value);
					}
				}
				catch(NullReferenceException)
				{
					//connections already destroyed.
				}
				StopAllCoroutines();
				isConnected = false;
				isClient = false;
				try
				{
					netObjs.Clear();
					connections.Clear();
					listener.Close();
				}
				catch(NullReferenceException)
				{
					netObjs = new Dictionary<int, Net_Objects>();
					connections = new Dictionary<int, Net_Connection>();
				}
			}
			//Restore the UI.
			this.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
			this.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
		}
		IEnumerator WaitForDisc()
		{
			if(isClient)
			{
				if(connections[0].kicky) {
					yield return new WaitForSeconds(0f);
					Destroy(transform.parent.gameObject);
					NetworkSceneChanger.MainMenu();
				} else {
					yield return new WaitUntil(() => connections[0].didDisc);
				}
				Disconnect(connections[0]);
			}
		}
		public void OnApplicationQuit()
		{
			LeaveGame();
		}

		//////////////////Object spawning and destruction /////////////////
		public void NetDestroyObject(int netId)
		{
			lock (_objLock)
			{
				try
				{
					Destroy(netObjs[netId].obj);
				}
				catch
				{
					//Means it was already destroyed
				}
				List<int> badCons = new List<int>();
				foreach (KeyValuePair<int, Net_Connection> entry in connections)
				{
					try
					{
						lock (_masterMsg)
						{
							masterMessage += "DELETE_" + netId + "\n";
						}
						/*entry.Value.connection.Send(
							Encoding.ASCII.GetBytes("DELETE_" + netId + "\n")
							);*/
					}
					catch
					{
						//connection died while trying to delete an object
						badCons.Add(entry.Key);
					}
				}
				for(int i =0; i < badCons.Count; i ++)
				{
					Disconnect(connections[badCons[i]]);
				}
				netObjs.Remove(netId);
			}
		}

		public GameObject NetCreateObject(int type, int ownMe, Vector3 initPos)
		{
			GameObject np;
			lock (_objLock)
			{
				np = GameObject.Instantiate(spawnPrefabs[type],initPos,Quaternion.identity);
				np.GetComponent<Net_ID>().owner = ownMe;
				np.GetComponent<Net_ID>().netId = objCounter;
				objCounter++;
				//CREATE_<TYPE>_<OWNER>_<NETID>
				string msg = "CREATE_" + type + "_" + ownMe +
					"_" + (objCounter - 1) + "_" + initPos.x.ToString("n2") + "_" + initPos.y.ToString("n2") + "_" + initPos.z.ToString("n2") + "\n";
				netObjs.Add(np.GetComponent<Net_ID>().netId,
					new Net_Objects(np, type, ownMe, objCounter - 1));
				lock (_masterMsg)
				{
					masterMessage += msg;
					
				}
			}
			return np;
		}
		
		public void setIPAddress(string s)
		{
			try
			{
				//Make sure it is valid.
				IPAddress.Parse(s);
				ipAddress = s;
			}
			catch(FormatException)
			{
				//Bad IP address.
			}
		}
	}
}
