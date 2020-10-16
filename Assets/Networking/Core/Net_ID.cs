using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Networking {
	public class Net_ID : MonoBehaviour
	{
		public bool isInit;
		public int netId=-1;
		public int owner=-2;
		public bool isLocalPlayer=false;
		//Must be set when game object is spawned.
		public Net_Core myCore;
		// Use this for initialization
		public string gameObjectMessages = "";
		public object _lock = new object();
		void Start()
		{
			myCore = GameObject.FindObjectOfType<Net_Core>();
			StartCoroutine(Init());
		}

		public IEnumerator Init()
		{
			yield return new WaitUntil(() => netId != -1 && owner != -2);
			if (myCore.localPlayerId == owner)
			{
				isLocalPlayer = true;
			}
			isInit = true;
		}

		public void Net_Update(string type, string var, string value)
		{
			//Get components for network behaviours
			//Destroy self if owner connection is done.
			try
			{
				if (myCore.isServer && myCore.connections.ContainsKey(owner) == false && owner != -1)
				{
					myCore.NetDestroyObject(netId);
				}
			}
			catch (System.NullReferenceException)
			{
				//Has not been initialized yet.  Ignore.
			}
			try
			{
				if(myCore == null)
				{
					myCore = GameObject.FindObjectOfType<Net_Core>();
				}
				if ((myCore.isServer && type == "COMMAND")
					|| (myCore.isClient && type == "UPDATE"))
				{
					Net_Component[] myNets = gameObject.GetComponents<Net_Component>();
					for (int i = 0; i < myNets.Length; i++)
					{
						myNets[i].handle_Message(var, value);
					}
				}
			}
			catch(System.Exception e)
			{
				Debug.Log("Caught Exception: " + e.ToString());
				//This can happen if myCore has not been set.  
				//I am not sure how this is possible, but it should be good for the next round.
			}
		}

		public void notifyDirty()
		{
				this.addMsg("DIRTY_"+netId);
		}

		public void addMsg(string msg)
		{
			//Debug.Log("Message WAS: " + gameObjectMessages);
			//May need to put race condition blocks here.
			lock (_lock)
			{
				gameObjectMessages += (msg + "\n");
			}
			//Debug.Log("Message IS NOW: " + gameObjectMessages);
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}
