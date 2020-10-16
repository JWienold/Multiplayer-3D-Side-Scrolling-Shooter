using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneChanger : MonoBehaviour {

	public static bool mainMenu = true;
	public static Controller c = null;
	public static int level = -1;
	
	// Use this for initialization
	void Start () {
	    GameObject[] objs = GameObject.FindGameObjectsWithTag("NetCore");

            if (objs.Length > 1)
            {
                //Destroy(this.gameObject);
            }
            else
            {
                SceneManager.sceneLoaded+=SceneChanger;
            }
	}
	
        public void SceneChanger(Scene s, LoadSceneMode m)
        {
            //Debug.Log("Called SceneChanger.");
            //Debug.Log("The scene name is " + s.name);
            //Debug.Log("The scene name for the first scene is " + SceneManager.GetSceneByBuildIndex(0).name);
            if(c != null) {
                c.Switch();
                c = null;
            }
        }
	
	// Update is called once per frame
	void Update () {
		
	}
	
	
	public static void MainMenu() {
	    if(!mainMenu) {
	    	SceneManager.LoadScene("MainMenu");
	    	level = -1;
	    }
	    mainMenu = true;
	}
	
	public static void Online() {
	    if(level < 0)
	        return;
	    if(mainMenu)
	    	SceneManager.LoadScene("Level" + (level+1));
	    mainMenu = false;
	}
	
	public static void CallMe(Controller c) {
	    NetworkSceneChanger.c = c;
	}
}
