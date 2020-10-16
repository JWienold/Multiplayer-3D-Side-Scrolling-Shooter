using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;

public class BulletController : Net_Component {
	
	Net_Rigidbody nrb;
	
	void Start()
	{
		nrb = GetComponent<Net_Rigidbody>();
		nrb.ServerCommand(3,100*transform.forward);
		StartCoroutine(WaitThenDestroy());
	}
			
	public override void handle_Message(string var, string value)
	{
	
	}
	
	void OnCollisionEnter(Collision collision) {
		Vector3 vv = transform.position;
		vv.z = 0;
		transform.position = vv;
		if(myCore.isServer) {
			myCore.NetDestroyObject(myId.netId);
			if(collision.gameObject.tag.Contains("Enemy")) {
				collision.gameObject.GetComponent<EnemyController>().TakeDamage(1f);
			}
		}
	}
	
	IEnumerator WaitThenDestroy() {
		yield return new WaitForSeconds(4);
		myCore.NetDestroyObject(myId.netId);
	}
}
