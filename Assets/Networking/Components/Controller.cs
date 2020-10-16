using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;

public class Controller : Net_Component {
	
	public Text text;
	public GameObject canvas;
	public bool ready = false;
	public bool switched = false;
	public int selectedLevel = 0;
	public string pname = "";
	
	GameObject playerObject;
	int selectedCharacter = 0;
	int selectedGun = 0;
	int state = 0;
	float timer = 0;
	float currentH;
	float previousH;
	bool enterPressed;
	bool inGame = false;
	
	void Start()
	{
		currentH = 0;
		previousH = 0;
		enterPressed = false;
		if(myCore.isServer) {
			//playerObject = myCore.NetCreateObject((int)Random.Range(1,7), myId.owner, new Vector3(0,0,(myId.owner % 10) - 5));
			//myCore.NetCreateObject(9, myId.owner, new Vector3(Random.Range(-10,10), 0, Random.Range(-10,10)));
		} else if(myId.isLocalPlayer) {
			
		}
		DontDestroyOnLoad(this.gameObject);
	}
		
	void Update() {
		if(myId.isLocalPlayer) {
			if(!inGame) {
				canvas.SetActive(true);
				currentH = Input.GetAxis("Horizontal");
				enterPressed = Input.GetKeyDown(KeyCode.Return);
				if(enterPressed)
					state++;
				
				RectTransform rt = text.rectTransform;
				Vector2 c = rt.anchoredPosition;
				c.y = -60 + Mathf.Sin(timer)*10f;
				rt.anchoredPosition = c;
				Vector3 vv = Vector3.zero;
				vv.z = Mathf.Cos(timer)*7.25f;
				rt.rotation = Quaternion.Euler(vv);
				CamMovementScript.goalRotation = Quaternion.identity;
				
				switch(state) {
				case 0:
					// selecting character
					text.text = "Select Your Character";
					if(currentH > 0.25f && previousH <= 0.25f) {
						selectedCharacter++;
						selectedCharacter %= 7;
					}
					
					if(currentH < -0.25f && previousH >= -0.25f) {
						selectedCharacter--;
						if(selectedCharacter < 0)
							selectedCharacter = 6;
					}
					CamMovementScript.goalPos.x = -9 + (3 * selectedCharacter);
					CamMovementScript.goalPos.y = 0.5f;
					CamMovementScript.goalPos.z = -3f;
					break;
				case 1:
					// selecting gun
					if(currentH > 0.25f && previousH <= 0.25f) {
						selectedGun++;
						selectedGun %= 3;
					}
					
					if(currentH < -0.25f && previousH >= -0.25f) {
						selectedGun--;
						if(selectedGun < 0)
							selectedGun = 2;
					}
					text.text = "Select Your Weapon";
					CamMovementScript.goalPos.x = -3f + (3 * selectedGun);
					CamMovementScript.goalPos.y = 4f;
					CamMovementScript.goalPos.z = -3f;
					break;
				case 2:
					// enter name
					text.text = "Enter Your Name";
					CamMovementScript.goalPos.x = 0;
					CamMovementScript.goalPos.y = 6f;
					CamMovementScript.goalPos.z = -3f;
					pname = ButtonMeme.pname;
					break;
				case 3:
					// select wanted level
					if(currentH > 0.25f && previousH <= 0.25f) {
						selectedLevel++;
						selectedLevel %= 3;
					}
					
					if(currentH < -0.25f && previousH >= -0.25f) {
						selectedLevel--;
						if(selectedLevel < 0)
							selectedLevel = 2;
					}
					text.text = "Select A Level";
					CamMovementScript.goalPos.x = -3f + (3 * selectedLevel);
					CamMovementScript.goalPos.y = 9f;
					CamMovementScript.goalPos.z = -3f;
					break;
				case 4:
					sendCommand("READY1", selectedCharacter.ToString() + "," + selectedGun.ToString() + "," + selectedLevel.ToString() + "," + pname);
					state++;
					ready = true;
					inGame = true;
					break;
				}
				
				previousH = currentH;
			} else {
				
				canvas.SetActive(false);
			}
			timer += 0.03f;
			
		}
		
		if(myCore == null)
			Destroy(gameObject);
	}
	
	public void Switch() {
		if(myCore.isServer) {
			//?_?
		} else if(myId.isLocalPlayer) {
			switched = true;
			sendCommand("SWITCHED","");
		}
	}
	
	public void AllReady() {
		if(myCore.isServer) {
			sendUpdate("ALLREADY","" + NetworkSceneChanger.level);
		} else { 
			//?_?
		}
	}
	
	public void AllLoaded() {
		if(myCore.isServer) {
			playerObject = myCore.NetCreateObject(selectedCharacter + 1, myId.owner, new Vector3((myId.owner % 10) - 5, 0, 0));
			StartCoroutine(SetPlayerStuff());
		} else {
			//?_?
		}
	}
			
	public override void handle_Message(string var, string value)
	{
		if(var == "READY1" && myCore.isServer) {
			selectedCharacter = int.Parse(value.Split(',')[0]);
			selectedGun = int.Parse(value.Split(',')[1]);
			selectedLevel = int.Parse(value.Split(',')[2]);
			pname = value.Split(new char[]{','},4)[3];
			ready = true;
			sendUpdate("INF", value);
		}
		
		if(var == "INF" && myCore.isClient) {
			selectedCharacter = int.Parse(value.Split(',')[0]);
			selectedGun = int.Parse(value.Split(',')[1]);
			selectedLevel = int.Parse(value.Split(',')[2]);
			pname = value.Split(new char[]{','},4)[3];
		}
		
		if(var == "SWITCHED" && myCore.isServer) {
			switched = true;
		}
		
		if(var == "ALLREADY" && myId.isLocalPlayer) {
			selectedLevel = int.Parse(value);
			NetworkSceneChanger.CallMe(this);
			NetworkSceneChanger.level = selectedLevel;
			NetworkSceneChanger.Online();
		}
	}
	
	public IEnumerator SetPlayerStuff() {
		yield return new WaitForSeconds(0.5f);
		PlayerMovementController pmc = playerObject.GetComponent<PlayerMovementController>();
		pmc.pname = pname;
		pmc.gunType = selectedGun+1;
	}
}
