using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class EnemyControllerWalking : EnemyController {

	void Start () {
		base.start();
		lastInput[1] = -1f;
	}
	
	void Update () {
		base.update();
	}
	
	public override IEnumerator FindTarget() {
		while(health > 0f) {
			yield return new WaitForSeconds(0.3f);
			RaycastHit rch;
			if(Physics.Raycast(transform.position + 0.5f*Vector3.up, transform.forward, out rch)) {
				if((rch.distance < 1.5f && rch.transform.tag.Contains("Wall")) || dmove.magnitude > 5f) {
					lastInput[1] = -lastInput[1];
					dmove = Vector3.zero;
				}
			}
		}
	}
	
	public override void handle_Message(string var, string value)
	{
		base.handle(var,value);
	}
	
	public override void Die() {
	
	}
}
