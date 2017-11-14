using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {
	float speed = 2.0f;
	Vector3 moveDirection = Vector3.zero;
	CharacterController controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate () {
		moveDirection = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Updown"), Input.GetAxis ("Vertical"));
		moveDirection = transform.TransformDirection (moveDirection);
		moveDirection *= speed;
		controller.Move (moveDirection * Time.deltaTime);
	}
}
