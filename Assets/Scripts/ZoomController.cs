using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class ZoomController : MonoBehaviour {
	float rate = 500.0f;
	bool flag = false;
	Vector3 initPos = Vector3.zero;
	Vector3 aimedPos = Vector3.zero;

	public GameObject bodyPivot; // cameras
	public StarGenerator sg;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        bool vrCtr = VRModeDetector.isInVR;

        bool vrInput = (Vector3.Distance(InputTracking.GetLocalPosition(VRNode.LeftHand), InputTracking.GetLocalPosition(VRNode.LeftEye)) < .1f);

        if ((vrCtr && vrInput) || Input.GetKeyDown (KeyCode.P) && flag == false)
		{
			Debug.Log ("Zoom in");
			initPos = bodyPivot.transform.position;
			aimedPos = initPos + Camera.main.transform.forward * rate;
			bodyPivot.transform.position = aimedPos;
			flag = true;
			sg.ignoreMovement = true;
		}
        if (((vrCtr && !vrCtr) || Input.GetKeyUp (KeyCode.P)) && flag == true)
		{
			Debug.Log ("Zoom out");
			bodyPivot.transform.position = initPos;
			flag = false;
			sg.ignoreMovement = false;
		}
	}
}
