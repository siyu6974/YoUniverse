using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour {

    //List<GameObject> planets = new List<GameObject>();

	// Use this for initialization
	void Start () {

        CoordinateManager.OnSystemChange += destroyAllPlanets;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void destroyAllPlanets() {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Planet")) {
            Destroy(go);
        } 
        //planets.Clear();
    }
}
