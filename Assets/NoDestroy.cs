using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDestroy : MonoBehaviour {

	void Start () {
		GameObject[] objs = GameObject.FindGameObjectsWithTag("NoPls");
		
		if (objs.Length > 1)
		{
			Destroy(this.gameObject);
		}
		else
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}
}
