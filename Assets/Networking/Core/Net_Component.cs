using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Networking {
	[RequireComponent(typeof(Net_ID))]
	public abstract class Net_Component: MonoBehaviour
	{
		//This determines if the script needs to send an update of its current 
		//game state.
		//This is only used by the server, when a new player joins.
		public bool isDirty;
		//a pointer back to the network core
		public Net_Core myCore;
		//a pointer to the current game objects id.
		public Net_ID myId;
		public void Awake()
		{
			myId = gameObject.GetComponent<Net_ID>();
			myCore = GameObject.FindObjectOfType<Net_Core>();
		}

		public void sendCommand(string var, string value)
		{
			var = var.Replace('_',' ');
			value = value.Replace('_', ' ');
			if(myCore != null && myCore.isClient)
			{
				string msg = "COMMAND_" + myId.netId + "_" + var + "_" + value;
				myId.addMsg(msg);
			}
		}
		public void sendUpdate(string var, string value)
		{
			var = var.Replace('_', ' ');
			value = value.Replace('_', ' ');
			if (myCore != null && myCore.isServer)
			{
				string msg = "UPDATE_" + myId.netId + "_" + var + "_" + value;
				myId.addMsg(msg);
			}
		}

		//Must be implemented individually by inherited behaviour.
		public abstract void handle_Message(string var, string value);
	}
}
