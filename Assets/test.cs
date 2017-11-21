using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = InputTracking.GetLocalPosition (VRNode.RightHand);
	}
}
