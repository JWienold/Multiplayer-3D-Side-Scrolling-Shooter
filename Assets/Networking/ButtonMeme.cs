using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;

public class ButtonMeme : MonoBehaviour {
	
	public Net_Core core;
	
	public static string pname;
	
	void Start() {
		pname = "NO_NAME";
		if(Thingy.server) {
			StartCoroutine(getIP());
		} else {
			StartCoroutine(getIP2());
		}
	}
	
	void Update() {
		
	}
	
	public void StartServer() {
		core.StartServer();
	}
	
	public void StartClient() {
		core.StartClient();
	}
	
	public void Disconnect() {
		core.LeaveGame();
		if(!NetworkSceneChanger.mainMenu) {
			Destroy(core.transform.parent.gameObject);
		} else if(Thingy.server) {
			StartServer();
		}
		NetworkSceneChanger.MainMenu();
		StartCoroutine(WaitMove());
	}
	
	public void UpdateIP(InputField input) {
		if(input.text.Trim() == "") {
			core.ipAddress = "127.0.0.1";
		} else {
			core.ipAddress = input.text;
		}
	}
	
	public IEnumerator WaitMove() {
		yield return new WaitForSeconds(1f);
		CamMovementScript.goalPos = new Vector3(0f,5.31f,-12.6f);
		CamMovementScript.goalRotation = Quaternion.Euler(28f,0f,0f);
	}
	
	public IEnumerator getIP() {
		yield return new WaitUntil(()=>{return core != null;});
		core.ipAddress = Thingy.serverIP;
		StartServer();
	}
	
	public IEnumerator getIP2() {
		yield return new WaitUntil(()=>{return core != null;});
		core.ipAddress = Thingy.serverIP;
	}
	
	public void UpdateName(InputField input) {
		if(input.text.Trim() == "") {
			pname = "NO_NAME";
		} else {
			pname = input.text;
		}
	}
	
}
