using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class EnemyControllerLookingShoot : EnemyController {

	protected Dictionary<float, GameObject> targets;
	
	void Start () {
		base.start();
		targets = new Dictionary<float, GameObject>();
	}
	
	void Update () {
		base.update();
	}
	
	public void FindEnemies()
	{
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag("Player");
		targets.Clear();
		Vector3 position = transform.position;
		foreach (GameObject go in gos)
		{
			Vector3 diff = go.transform.position - position;
			float curDistance = diff.magnitude;
			if(!targets.ContainsKey(curDistance))
				targets.Add(curDistance, go);
		}
	}
	
	public override IEnumerator FindTarget() {
		while(health > 0f) {
			yield return new WaitForSeconds(0.3f);
			FindEnemies();
			float[] vals = new float[targets.Count];
			targets.Keys.CopyTo(vals,0);
			Array.Sort(vals, (x,y) => {return (int)Mathf.Sign(x-y);});
			lastInput[1] = 0f;
			lastInput[0] = 0f;
			for(int i = 0; i < vals.Length && vals[i] < 10f; i++) {
				GameObject tempObj = targets[vals[i]];
				if(Mathf.Sign(transform.forward.z) != Mathf.Sign(tempObj.transform.position.x - transform.position.x))
					continue;
				RaycastHit rch;
				if(Physics.Raycast(transform.position + 0.5f*Vector3.up, tempObj.transform.position - transform.position, out rch)) {
					if(rch.transform.gameObject == tempObj) {
						//shoot player
						
						break;
					} else {
						
					}
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
