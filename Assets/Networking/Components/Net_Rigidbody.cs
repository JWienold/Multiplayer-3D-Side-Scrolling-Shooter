using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class Net_Rigidbody : Net_Component {

	//Don't worry about these, I wanted my Net_Rigidbody to look like a normal Rigidbody
	public float mass = 1;
	public float drag = 0;
	public float angularDrag = 0.05f;
	public bool useGravity = true;
	public bool isKinematic = false;
	public RigidbodyInterpolation interpolate = RigidbodyInterpolation.None;
	public CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;
	//public RigidbodyConstraints constraints = RigidbodyConstraints.None;
	
	//These determine what to send from the server to the client
	public bool sendAngularVelocity = true;
	public bool sendRotation = true;
	public bool sendVelocity = true;
	
	public bool lockRotation = false;
	
	Rigidbody rb;
	Vector3 angVel;
	Vector3 pos;
	Vector3 rot;
	Vector3 vel;
	Vector3 diff = Vector3.zero;
	Vector3 pdiff = Vector3.zero;
	
	void Start () {
		//dynamically add Rigidbody component
		rb = gameObject.AddComponent<Rigidbody>() as Rigidbody;
		
		//set all values provided by user
		rb.mass = mass;
		rb.drag = drag;
		rb.angularDrag = angularDrag;
		rb.useGravity = useGravity;
		rb.isKinematic = isKinematic;
		rb.interpolation = interpolate;
		rb.collisionDetectionMode = collisionDetection;
		//rb.constraints = constraints;
		
		//default values for all will be (0,0,0)
		angVel = Vector3.zero;
		pos = Vector3.zero;
		rot = Vector3.zero;
		vel = Vector3.zero;
		
		if(lockRotation) {
			rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
		}
		
		//start slow update
		StartCoroutine(slowUpdate());
	}
	
	void Update () {
		//unused
		diff = Vector3.Lerp(diff, Vector3.zero, 0.1f);
		transform.position += (pdiff - diff);
		pdiff = diff;
	}
	
	public void ServerCommand(int a, Vector3 b) {
		//don't worry about this :P
		ServerCommand(a,b.x,b.y,b.z);
	}
	
	public void ServerCommand(int a, float b, float c, float d) {
		//You can split this into multiple methods, I didn't feel like it
		switch(a) {
		case 0: //0 = change angular velocity
			rb.angularVelocity = new Vector3(b,c,d);
			break;
		case 1: //1 = change position
			transform.position = new Vector3(b,c,d);
			break;
		case 2: //2 = change rotation
			transform.eulerAngles = new Vector3(b,c,d);
			break;
		case 3: //3 = change velocity
			rb.velocity = new Vector3(b,c,d);
			break;
		case 4: //4 = apply torque
			rb.AddForce(b,c,d);
			break;
		case 5:
			Vector3 ttt = rb.velocity;
			switch((int)Mathf.Floor(b)) {
			case 0:
				ttt.x = c;
				break;
			case 1:
				ttt.y = c;
				break;
			case 2:
				ttt.z = c;
				break;
			}
			rb.velocity = ttt;
			break;
		}
	}

	public IEnumerator slowUpdate()
	{
		while (true)
		{
			//only send from server to client
			if (myCore.isServer)
			{
				if ((rb.angularVelocity != angVel || isDirty) && sendAngularVelocity)
				{
					//send angular velocity to client
					angVel = rb.angularVelocity;
					sendUpdate("ANG", angVel.x.ToString() + "," + angVel.y.ToString() + "," + angVel.z.ToString());
				}
				if (transform.position != pos || isDirty || (rb.velocity != vel && sendVelocity))
				{
					//send position to client
					pos = transform.position;
					sendUpdate("POS", pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString());
					if (sendVelocity)
					{
						//send velocity to client
						vel = rb.velocity;
						sendUpdate("VEL", vel.x.ToString() + "," + vel.y.ToString() + "," + vel.z.ToString());
					}
				}
				if ((transform.eulerAngles != rot || isDirty) && sendRotation)
				{
					//send rotation to client
					rot = transform.eulerAngles;
					sendUpdate("ROT", rot.x.ToString() + "," + rot.y.ToString() + "," + rot.z.ToString());
				}
				
				isDirty = false;
			}
			
			yield return new WaitForSeconds(0.05f);
		}
	}
	
	public override void handle_Message(string var, string value)
	{
		if(var == "ANG" && myCore.isClient)
		{
			//if client recieves angular velocity message from the server
			angVel.x = float.Parse(value.Split(',')[0]);
			angVel.y = float.Parse(value.Split(',')[1]);
			angVel.z = float.Parse(value.Split(',')[2]);
			//store it to the Rigidbody
			rb.angularVelocity = angVel;
			isDirty = false;
		}
		else if(var == "POS" && myCore.isClient)
		{
			//if client recieves position message from the server
			pos.x = float.Parse(value.Split(',')[0]);
			pos.y = float.Parse(value.Split(',')[1]);
			pos.z = float.Parse(value.Split(',')[2]);
			//store it to the Transform
			//if((transform.position - pos).magnitude >= 1)
			//transform.position = pos;
			diff = pos - transform.position;
			if(diff.magnitude > 10 || isDirty) {
				transform.position = pos;
				diff = Vector3.zero;
			}
			pdiff = diff;
			isDirty = false;
		}
		else if(var == "ROT" && myCore.isClient)
		{
			//if client recieves rotation message from the server
			rot.x = float.Parse(value.Split(',')[0]);
			rot.y = float.Parse(value.Split(',')[1]);
			rot.z = float.Parse(value.Split(',')[2]);
			//stoore it to the Transform
			transform.eulerAngles = rot;
			isDirty = false;
		}
		else if(var == "VEL" && myCore.isClient)
		{
			//if client recieves velocity message from the server
			vel.x = float.Parse(value.Split(',')[0]);
			vel.y = float.Parse(value.Split(',')[1]);
			vel.z = float.Parse(value.Split(',')[2]);
			//store it to the Rigidbody
			rb.velocity = vel;
			isDirty = false;
		}
	}
}

