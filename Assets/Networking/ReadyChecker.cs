using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;

public class ReadyChecker : MonoBehaviour {
	
	bool ready = false;
	bool wasReady = false;
	bool switched = false;
	bool hasGot = false;
	Net_Core core;
	
	// Use this for initialization
	void Start () {
		core = GetComponent<Net_Core>();
		StartCoroutine(slowUpdate());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	IEnumerator slowUpdate() {
		while(true) {
			yield return new WaitForSeconds(0.5f);
			if(core.isServer && core.connections.Count > 0) {
				if(!wasReady) {
					Controller[] controllers = GameObject.FindObjectsOfType<Controller>();
					ready = true;
					foreach(Controller c in controllers) {
						if(!c.ready) {
							ready = false;
							break;
						}
					}
					
					if(ready) {
						wasReady = true;
						NetworkSceneChanger.level = controllers[Random.Range(0,controllers.Length - 1)].selectedLevel;
						foreach(Controller c in controllers) {
							c.AllReady();
						}
						NetworkSceneChanger.Online();
					}
				} else {
					if(!switched) {
						Controller[] controllers = GameObject.FindObjectsOfType<Controller>();
						switched = true;
						foreach(Controller c in controllers) {
							if(!c.switched) {
								switched = false;
								break;
							}
						}
						
						if(switched) {
							foreach(Controller c in controllers) {
								c.AllLoaded();
							}
							SpawnEnemies();
						}
					} else {
						if(!hasGot) {
							EnemyController[] cc = GameObject.FindObjectsOfType<EnemyController>();
							bool gotem = true;
							foreach(EnemyController ec in cc) {
								if(!ec.dead) {
									gotem = false;
									break;
								}
							}
							if(gotem) {
								hasGot = true;
								core.NetCreateObject(10, -1, Vector3.zero);
								StartCoroutine(FuckEm());
							} else {
								PlayerMovementController[] pp = GameObject.FindObjectsOfType<PlayerMovementController>();
								bool ppded = true;
								foreach(PlayerMovementController p in pp) {
									if(p.Health > 0) {
										ppded = false;
										break;
									}
								}
								if(ppded) {
									core.NetCreateObject(11, -1, Vector3.zero);
									StartCoroutine(FuckEm());
									hasGot = true;
								}
							}
						}
					}
				}
			} else if(core.isServer && !NetworkSceneChanger.mainMenu) {
				core.LeaveGame();
				Destroy(core.transform.parent.gameObject);
				NetworkSceneChanger.MainMenu();
			} else {
				ready = false;
				wasReady = false;
				switched = false;
			}
		}
	}
	
	public IEnumerator FuckEm() {
		yield return new WaitForSeconds(10f);
		core.LeaveGame();
		yield return new WaitForSeconds(0.5f);
		Destroy(core.transform.parent.gameObject);
		NetworkSceneChanger.MainMenu();
	}
	
	public void SpawnEnemies() {
		switch(NetworkSceneChanger.level) {
		case 0:
			/*
			Level 1:
0 to 44.12, 3.22, 0 
33.96 to 44.12, 7.15, 0
8.22 to 26.79, 9.57, 0
-0.38, 12.73, 0
8.22 to 26.79, 15.5, 0
26.79 to 35.75, 19.69, 0
26.79 to 41.65, 23.89, 0
6.24 to 15.47, 26.45, 0
			*/
			SpawnAt(0f-9.12f, 44.12f-9.12f, 3.22f-2.22f);
			SpawnAt(33.96f-9.12f, 44.12f-9.12f, 7.15f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 9.57f-2.22f);
			SpawnAt(-0.38f-9.12f, -0.38f-9.12f, 12.73f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 15.5f-2.22f);
			SpawnAt(26.79f-9.12f, 35.75f-9.12f, 19.69f-2.22f);
			SpawnAt(26.79f-9.12f, 41.65f-9.12f, 23.89f-2.22f);
			SpawnAt(6.24f-9.12f, 15.47f-9.12f, 26.45f-2.22f);

			SpawnAt(0f-9.12f, 44.12f-9.12f, 3.22f-2.22f);
			SpawnAt(33.96f-9.12f, 44.12f-9.12f, 7.15f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 9.57f-2.22f);
			SpawnAt(-0.38f-9.12f, -0.38f-9.12f, 12.73f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 15.5f-2.22f);
			SpawnAt(26.79f-9.12f, 35.75f-9.12f, 19.69f-2.22f);
			SpawnAt(26.79f-9.12f, 41.65f-9.12f, 23.89f-2.22f);
			SpawnAt(6.24f-9.12f, 15.47f-9.12f, 26.45f-2.22f);

			SpawnAt(0f-9.12f, 44.12f-9.12f, 3.22f-2.22f);
			SpawnAt(33.96f-9.12f, 44.12f-9.12f, 7.15f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 9.57f-2.22f);
			SpawnAt(-0.38f-9.12f, -0.38f-9.12f, 12.73f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 15.5f-2.22f);
			SpawnAt(26.79f-9.12f, 35.75f-9.12f, 19.69f-2.22f);
			SpawnAt(26.79f-9.12f, 41.65f-9.12f, 23.89f-2.22f);
			SpawnAt(6.24f-9.12f, 15.47f-9.12f, 26.45f-2.22f);
			
			SpawnAt(0f-9.12f, 44.12f-9.12f, 3.22f-2.22f);
			SpawnAt(33.96f-9.12f, 44.12f-9.12f, 7.15f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 9.57f-2.22f);
			SpawnAt(-0.38f-9.12f, -0.38f-9.12f, 12.73f-2.22f);
			SpawnAt(8.22f-9.12f, 26.79f-9.12f, 15.5f-2.22f);
			SpawnAt(26.79f-9.12f, 35.75f-9.12f, 19.69f-2.22f);
			SpawnAt(26.79f-9.12f, 41.65f-9.12f, 23.89f-2.22f);
			SpawnAt(6.24f-9.12f, 15.47f-9.12f, 26.45f-2.22f);
			break;
		case 1:
			/*
			Level 2:
-20.57 to -4, 0.49, 0

2.88 to 17.11, 5.52, 0

-1.97 to -4.24, 10.17, 0
-7.01 to -9.14, 10.17, 0
-11.93 to -14.28, 10.17, 0
-16.64 to -20.16, 10.17, 0

4.78 to 15.58, 13.65, 0

-1.4 to -4.88, 17,21, 0
-13.71 to -10.21, 21.15, 0
-21.55 to -8.4, 28.37, 0
2 to 13.02, 31.48, 0
7 to -21.55, 40.36, 0
			*/
			SpawnAt(-20.57f, -4f, 0.49f);
			SpawnAt(2.88f, 17.11f, 5.52f);
			SpawnAt(-14.28f, -11.93f, 10.17f);
			SpawnAt(-20.16f, -16.64f, 10.17f);
			SpawnAt(4.78f, 15.58f, 13.65f);
			SpawnAt(-4.88f, -1.4f, 17.21f);
			SpawnAt(-13.71f, -10.21f, 21.15f);
			SpawnAt(-21.55f, -8.4f, 28.37f);
			SpawnAt(2f, 13.02f, 31.48f);
			SpawnAt(-21.55f, 7f, 40.36f);

			SpawnAt(-20.57f, -4f, 0.49f);
			SpawnAt(2.88f, 17.11f, 5.52f);
			SpawnAt(4.78f, 15.58f, 13.65f);
			SpawnAt(-4.88f, -1.4f, 17.21f);
			SpawnAt(-13.71f, -10.21f, 21.15f);
			SpawnAt(-21.55f, -8.4f, 28.37f);
			SpawnAt(2f, 13.02f, 31.48f);
			SpawnAt(-21.55f, 7f, 40.36f);

			SpawnAt(-20.57f, -4f, 0.49f);
			SpawnAt(2.88f, 17.11f, 5.52f);
			SpawnAt(-13.71f, -10.21f, 21.15f);
			SpawnAt(-21.55f, -8.4f, 28.37f);
			SpawnAt(2f, 13.02f, 31.48f);
			SpawnAt(-21.55f, 7f, 40.36f);

			SpawnAt(-20.57f, -4f, 0.49f);
			SpawnAt(2.88f, 17.11f, 5.52f);
			SpawnAt(-4.24f, -1.97f, 10.17f);
			SpawnAt(-9.14f, -7.01f, 10.17f);
			SpawnAt(-14.28f, -11.93f, 10.17f);
			SpawnAt(-20.16f, -16.64f, 10.17f);
			SpawnAt(-13.71f, -10.21f, 21.15f);
			SpawnAt(-21.55f, -8.4f, 28.37f);
			SpawnAt(2f, 13.02f, 31.48f);
			SpawnAt(-21.55f, 7f, 40.36f);
				break;
		case 2:
			/*
			Level 3:
17 to 0, -0.38, 0
-1.44 to -7.33, 4.92, 0
-19.08 to 4.16, 9.28, 0
16.68 to -9.84, 13.9, 0
			*/
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			SpawnAt(0f, 17f, -0.38f);
			SpawnAt(-7.33f, -1.44f, 4.92f);
			SpawnAt(-19.08f, 4.16f, 9.28f);
			SpawnAt(-9.84f, 16.68f, 13.9f);
			break;
		}
	}
	
	void SpawnAt(float x1, float x2, float y) {
		core.NetCreateObject(9, -1, new Vector3(Random.Range(x1,x2), y, 0));
	}
}
