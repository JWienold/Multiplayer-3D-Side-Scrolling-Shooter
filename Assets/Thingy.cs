using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Thingy : MonoBehaviour {
	
	public static bool server = false;
	public static string serverIP = "127.0.0.1";
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void SetServer() {
		server = true;
		SwitchScene();
	}
	
	public void SetClient() {
		SwitchScene();
	}
	
	public void SwitchScene() {
		SceneManager.LoadScene("MainMenu");
	}
	
	public void UpdateIP(InputField input) {
		if(input.text.Trim() == "") {
			serverIP = "127.0.0.1";
		} else {
			serverIP = input.text;
		}
	}
}
