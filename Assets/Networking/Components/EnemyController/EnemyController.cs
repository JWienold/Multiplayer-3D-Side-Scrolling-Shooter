using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;

public abstract class EnemyController : Net_Component {
	
	protected Animator anim;
	public float animSpeed = 1.5f;
	public float health = 10f;
	public float Health {
		get {
			return health;
		}
		
		set {
			if(health > value && value <= 0) {
				Die();
			} 
			health = value;
		}
	}
	
	public bool dead = false;
	
	protected Net_Rigidbody nrb;
	protected Vector3 lastInput;
	protected Vector3 lastInput1;
	protected Vector3 interp;
	protected Vector3 dmove;
	protected Vector3 ploc;
	protected float angleGoal;
	protected float curAngle;
	protected bool isJumping;
	
	protected void start() {
		nrb = GetComponent<Net_Rigidbody>();
		anim = GetComponent<Animator>();
		
		interp = Vector3.zero;
		lastInput = Vector3.zero;
		lastInput1 = Vector3.zero;
		dmove = Vector3.zero;
		ploc = transform.position;
		curAngle = 270;
		angleGoal = 270;
		isJumping = false;
		
		StartCoroutine(FindTarget());
		StartCoroutine(SlowUpdate());
	}
	
	protected void update() {
		//if(myCore.isServer) {
			Vector3 vv = transform.position;
			vv.z = 0;
			transform.position = vv;
			interp = Vector3.Lerp(interp, lastInput, 0.6f);
			anim.SetFloat("Speed", Mathf.Abs(interp[1]));
			if(interp[1] < 0) {
				angleGoal = 270;
			} else if(interp[1] > 0) {
				angleGoal = 90;
			}
			curAngle = Mathf.LerpAngle(curAngle, angleGoal, 0.4f);
			Vector3 ttt = transform.eulerAngles;
			ttt.y = curAngle;
			transform.eulerAngles = ttt;
			
			if(lastInput[0] > 0.25f && !isJumping) {
				isJumping = true;
				nrb.ServerCommand(5, 1f, 12f, 0f);
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
			dmove += (transform.position - ploc);
			ploc = transform.position;
		//}
	}
	
	protected void handle(string var, string value) {
		if(myId.isLocalPlayer) {
			
		}
		
		if(myCore.isClient) {
			if(var == "PMOV") {
				float v = float.Parse(value.Split(',')[0]);
				float h = float.Parse(value.Split(',')[1]);
				interp = lastInput;
				lastInput = new Vector3(v,h,0);
				isDirty = false;
			}
			if(var == "EXPLODE") {
				GetComponent<ToonCharacterController>().Decapitate(true, 0f, Vector3.up);
			}
		}
		
		if(myCore.isServer) {
			
		}
	}
	
	protected IEnumerator SlowUpdate() {
		while(health > 0f) {
			if(myCore.isServer) {
				if(lastInput != lastInput1) {
					lastInput1 = lastInput;
					sendUpdate("PMOV", lastInput[0].ToString("n2") + "," + lastInput[1].ToString("n2"));
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	public abstract IEnumerator FindTarget();
	public abstract void Die();
	public void TakeDamage(float damage) {
		dead = true;
		GetComponent<ToonCharacterController>().Decapitate(true, 0f, Vector3.up);
		sendUpdate("EXPLODE", "");
	}
}
