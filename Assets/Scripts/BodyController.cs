using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class BodyController : MonoBehaviour {
	public GameObject master;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = new Vector3 (0, -0.65f, 0);
//        offset = transform.position - master.transform.position;
		Debug.Log (offset);
	}

	void Update () {
        transform.position = master.transform.position + offset;
//        if (!VRModeDetector.isInVR)
            transform.rotation = master.transform.rotation;
	}
}
