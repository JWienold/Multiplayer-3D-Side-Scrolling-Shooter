using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovementScript : MonoBehaviour {

	public static Vector3 goalPos;
	public static Vector3 curPos;
	public static Quaternion goalRotation;
	
	Vector3 bounce;
	float timer;
	
	// Use this for initialization
	void Start () {
		goalPos = transform.position;
		curPos = transform.position;
		bounce = Vector3.zero;
		timer = 0;
		goalRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		curPos = Vector3.Lerp(curPos, goalPos, 0.2f);
		bounce.y = 0.0625f * Mathf.Sin(timer);
		transform.position = curPos + bounce;
		transform.rotation = Quaternion.Lerp(transform.rotation, goalRotation, 0.2f);
		timer += 0.025f;
	}
}
