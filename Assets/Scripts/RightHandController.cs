using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class RightHandController : MonoBehaviour {
	public GameObject bodyPivot;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		bool vrCtr = VRModeDetector.isInVR;

		if (vrCtr) {
			transform.position = InputTracking.GetLocalPosition (VRNode.RightHand);
			transform.rotation = InputTracking.GetLocalRotation (VRNode.RightHand);
		}
	}
}
