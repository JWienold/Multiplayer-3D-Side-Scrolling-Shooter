using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class RandomEnemyController : Net_Component {
	
	int type = 0;
	// Use this for initialization
	void Start () {
		if(myCore.isServer) {
			type = Random.Range(1,5);
			SpawnType();
		} else if(myCore.isClient) {
			StartCoroutine(BGSpawnType());
		}
		StartCoroutine(SlowUpdate());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public IEnumerator SlowUpdate() {
		while(true) {
			if(isDirty && myCore.isServer) {
				sendUpdate("TYPE", type.ToString());
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	public IEnumerator BGSpawnType() {
		yield return new WaitUntil(() => type != 0);
		SpawnType();
	}
	
	public void SpawnType() {
		switch(type) {
		case 1:
			gameObject.AddComponent<EnemyControllerLooking>();
			break;
		case 2:
			/*gameObject.AddComponent<EnemyControllerLookingShoot>();
			break;
		case 3:*/
			gameObject.AddComponent<EnemyControllerTracking>();
			break;
		/*case 4:
			gameObject.AddComponent<EnemyControllerTrackingShoot>();
			break;
		case 5:*/
		default:
			gameObject.AddComponent<EnemyControllerWalking>();
			break;
		}
	}
	
	public override void handle_Message(string var, string value)
	{
		if(var == "TYPE" && myCore.isClient) {
			type = int.Parse(value);
		}
	}
}
