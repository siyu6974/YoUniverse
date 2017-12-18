using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperDrive : MonoBehaviour {

    public LaserPointer pointer;

    private StarData lockedStar;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    bool lockStar() {
        if (Input.GetKey(KeyCode.L) && pointer.pointed != null) {
            lockedStar = (StarData)pointer.pointed;
            return true;
        }
        return false;
    }
}
