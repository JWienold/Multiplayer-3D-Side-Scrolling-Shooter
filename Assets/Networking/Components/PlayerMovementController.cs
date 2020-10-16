using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
public class PlayerMovementController : Net_Component {
	
	public float animSpeed = 1.5f;
	public int gunType = 0;
	public float Health {
		get {return health;}
		set {
			if(myCore.isServer) {
				sendUpdate("HEALTH", value.ToString());
			}
			if(value <= 0f) {
				StartCoroutine(Dead());
			}
			health = value;
		}
	}
	public string pname = "";
	public bool canDamage = true;
	public Text nameText;
	public Text nameText2;
	
	private Animator anim;
	private float health = 10f;

	float angleGoal;
	float curAngle;
	bool butts = true;
	bool jumping;
	bool isJumping {
		get { 
			return jumping;
		}
		
		set {
			if(value && myCore.isClient)
				sendCommand("JUMP","PLS");
			jumping = value;
		}
	}
	
	Vector3 lastInput;
	Vector3 lastInput1;
	Vector3 interp;
	
	Net_Rigidbody nrb;
	
	void Start () {
		
		anim = gameObject.GetComponent<Animator>();					  
		nrb = GetComponent<Net_Rigidbody>();
		
		angleGoal = 90;
		curAngle = 90;
		isJumping = false;
		lastInput = Vector3.zero;
		lastInput1 = Vector3.zero;
		interp = Vector3.zero;

		StartCoroutine(LoadGun());
		StartCoroutine(slowUpdate());
		
		anim.applyRootMotion = false;
	}
	
	void Update () {
		//unused
		//if(myId.isLocalPlayer) {
			//nrb.ServerCommand(5, 0f, (Mathf.Abs(interp[1]) > 0.15f) ? (interp[1] * 6.5f) : 0f, 0f);
		//}
	}
	
	void FixedUpdate ()
	{
		if(Health > 0) {
			Vector3 vv = transform.position;
			vv.z = 0;
			transform.position = vv;
			nameText.text = pname + "\nHealth: " + Health.ToString("n2");
			nameText2.text = pname + "\nHealth: " + Health.ToString("n2");
			interp = Vector3.Lerp(interp, lastInput, 0.6f);
			anim.SetFloat("Speed", Mathf.Abs(interp[1]));
			if(interp[1] < 0) {
				angleGoal = 270;
			} else if(interp[1] > 0) {
				angleGoal = 90;
			}
			curAngle = Mathf.LerpAngle(curAngle, angleGoal, 0.4f);
			nrb.ServerCommand(2, 0, curAngle, 0);
			anim.SetFloat("Direction", 0);
			if(myCore.isServer)
				nrb.ServerCommand(5, 0f, 6.5f * lastInput[1], 0f);
		
			if(lastInput[0] > 0.25f && !isJumping) {
				isJumping = true;
				nrb.ServerCommand(5, 1f, 10f, 0f);
			}
		
			anim.speed = animSpeed;
		
			if(Input.GetButtonDown("Jump"))
			{	
				Shoot();
			}
		
			if(isJumping)
			{
				Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
				RaycastHit hitInfo = new RaycastHit();
			
				if (Physics.Raycast(ray, out hitInfo))
				{
					if (hitInfo.distance < 1.1f) {
						isJumping = false;
					}
				}
			}
		
			if(myId.isLocalPlayer) {
				CamMovementScript.goalPos.x = transform.position.x;
				CamMovementScript.goalPos.y = Mathf.Max(3f, (transform.position.y + 2.5f));
				CamMovementScript.goalPos.z = -14f;
			} else if(myCore.isServer) {
				//nrb.ServerCommand(5, 0f, (Mathf.Abs(interp[1]) > 0.15f) ? (interp[1] * 6.5f) : 0f, 0f);
			}
		} else {
			if(butts) {
				butts = false;
				//GetComponent<ToonCharacterController>().Decapitate(false, 0f, Vector3.up);
			}
		}
	}
	
	void Shoot() {
		if(myId.isLocalPlayer) {
			sendCommand("SHOOT","PLS");
		}
	}
	
	public IEnumerator slowUpdate()
	{
		while (true)
		{
			//only send from server to client
			if (myCore.isServer)
			{
				sendUpdate("PMOV", lastInput[0].ToString("n2") + "," + lastInput[1].ToString("n2"));
			} else if(myId.isLocalPlayer) {
				float h = Input.GetAxis("Horizontal");
				float v = Input.GetAxis("Vertical");
				Vector3 temp = new Vector3(v,h,0);
				if(temp != lastInput1) {
					lastInput1 = temp;
					sendCommand("PMOV", v.ToString("n2") + "," + h.ToString("n2"));
				}
			}
			
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	void OnCollisionStay(Collision collision) {
		if(myCore.isServer && collision.gameObject.tag.Contains("Enemy") && canDamage) {
			canDamage = false;
			StartCoroutine(ResetDamage());
			Health -= 0.25f;
		}
	}
	
	public IEnumerator ResetDamage() {
		yield return new WaitForSeconds(0.25f);
		canDamage = true;
	}
	
	public IEnumerator Dead() {
		yield return new WaitForSeconds(0.2f);
		GetComponent<ToonCharacterController>().Decapitate(false, 0f, Vector3.up);
	}
	
	public IEnumerator LoadGun() {
		yield return new WaitUntil(() => { return gunType != 0; });
		Transform t = transform.Find("Root/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/RightHandIndex1");
		GameObject q = Instantiate(Resources.Load<GameObject>("Gun" + gunType), t.position, t.rotation);
		q.transform.parent = t;
		q.transform.localPosition = new Vector3(-0.07f, 0.05f, 0.03f);
		q.transform.localRotation = Quaternion.Euler(60,0,45);
		if(myCore.isServer) {
			sendUpdate("GUN", gunType.ToString());
			sendUpdate("NAM", pname);
		}
	}
	
	public override void handle_Message(string var, string value)
	{
		if(var == "PMOV" && myCore.isServer) {
			float v = float.Parse(value.Split(',')[0]);
			float h = float.Parse(value.Split(',')[1]);
			interp = lastInput;
			lastInput = new Vector3(v,h,0);
			isDirty = false;
		}
		if(var == "PMOV" && myCore.isClient) {
			float v = float.Parse(value.Split(',')[0]);
			float h = float.Parse(value.Split(',')[1]);
			interp = lastInput;
			lastInput = new Vector3(v,h,0);
			isDirty = false;
		}
		if(var == "HEALTH" && myCore.isClient) {
			Health = float.Parse(value);
			nameText.text = pname + "\nHealth: " + Health.ToString("n2");
			nameText2.text = pname + "\nHealth: " + Health.ToString("n2");
		}
		if(var == "SHOOT" && myCore.isServer) {
			GameObject te = myCore.NetCreateObject(8, myId.owner, transform.position + (transform.forward * 0.75f) + transform.up);
			te.transform.rotation = Quaternion.Euler(0,angleGoal,0);
		}
		if(var == "JUMP" && myCore.isServer) {
			isJumping = true;
		}
		if(var == "GUN" && myCore.isClient) {
			gunType = int.Parse(value);
		}
		if(var == "NAM" && myCore.isClient) {
			if(value != "")
				pname = value;
		}
	}
}

