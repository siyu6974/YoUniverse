using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour {
	CharacterController controller;
	float rate = 3.0f;
	bool flag = false;
	Vector3 initPos = Vector3.zero;
	Vector3 aimedPos = Vector3.zero;

	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController> ();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown (KeyCode.P))
		{
			Debug.Log ("Zoom in");
			initPos = controller.transform.position;
			aimedPos = initPos + Camera.main.transform.forward * rate;
			controller.transform.position = aimedPos;
			flag = true;

		}
		if (Input.GetKeyUp (KeyCode.P) && flag == true)
		{
			Debug.Log ("Zoom out");
			controller.transform.position = initPos;
			flag = false;
		}
	}
}
