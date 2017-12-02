using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class RightHandController : MonoBehaviour {
	public GameObject bodyPivot;
	Vector3 offset;
	// Use this for initialization
	void Start () {
		offset = new Vector3 (0.626f, 0, 0.448f);
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
