﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class CameraController : MonoBehaviour {
    float speed = 2.0f;
    float height;
    float offsetHeight = 3.0f;
//    float flyPreparationHeight = 2.0f;
    float distanceForLanding = 5.0f;
    Vector3 moveDirection = Vector3.zero;
	Vector3 targetPosition = Vector3.zero;
	Quaternion targetRotation;

	Vector3 landingDirection;

    enum characterStates { flying = 1, inSpace = 2, landing = 3, onOrbit = 4 };
    characterStates state;
	enum landingPhases {notLanding = 0, preparing = 1, turing = 2, getingDown = 3};
	landingPhases phase;

    RaycastHit? hitInfo;
    Ray ray;

    public Camera cam;
    int layerMask;
    // Use this for initialization
    void Start() {
        state = characterStates.onOrbit;
		phase = landingPhases.notLanding;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    void FixedUpdate() {
		// Debug for checking down direction for character
//		Vector3 directionDown = Camera.main.transform.up * (-1);
//		Debug.DrawRay(cam.transform.position, directionDown * 1000f, Color.blue);
		Vector3 directionDown = transform.up * (-1);
//		landingDirection = directionDown;
		Debug.DrawRay(transform.position, directionDown * 1000f, Color.blue);

		changeCharacterState();
    }

    void useRay(Vector3 direction) {
        ray = new Ray(cam.transform.position, direction);
        // Debug for checking forward direction for character
		Debug.DrawRay(cam.transform.position, ray.direction * 1000f, Color.red);

        RaycastHit hit;
        bool rayCasted = Physics.Raycast(ray, out hit, cam.farClipPlane, layerMask);
        if (rayCasted)
            hitInfo = hit;
        else
            hitInfo = null;
    }

    float getHeightToSurface() {
        float distance = -1.0f;
		Ray landingRay = new Ray (transform.position, transform.up * (-1));
		Debug.DrawRay (transform.position, landingRay.direction * 1000f, Color.yellow);

		RaycastHit landingHit;
		bool rayCasted = Physics.Raycast (landingRay, out landingHit, cam.farClipPlane, layerMask);
		if (rayCasted)
			hitInfo = landingHit;
		else
			hitInfo = null;
		
//        distance = hitInfo?.distance;
        if (hitInfo != null)
            distance = ((RaycastHit)hitInfo).distance;
        return distance;
    }

    bool detectToLand() {
//        useRay(Camera.main.transform.forward);
		useRay(transform.forward);
        if (hitInfo != null && ((RaycastHit)hitInfo).distance <= distanceForLanding)
            return true;
        else
            return false;
    }

    void changeCharacterState() {
        switch (state) {
            case characterStates.flying: {
					Debug.Log ("Flying");
                    if (Mathf.Abs(Input.GetAxis("Updown")) <= 0.000001) {
                        state = characterStates.inSpace;
                    } else {
//                        height = getHeightToSurface();
//                        if (height < flyPreparationHeight) {
//                        	moveDirection = new Vector3 (0, Input.GetAxis ("Updown"), 0);
//                        	moveDirection = transform.TransformDirection (moveDirection);
//                        	moveDirection *= speed;
//                        	transform.Translate (moveDirection * Time.deltaTime);
//                        } else {
                        
						moveDirection = Camera.main.transform.forward;
                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime);
                        //}
                        bool toLand = detectToLand();
                        if (toLand) {
                            state = characterStates.landing;
							phase = landingPhases.preparing;
                        }
                    }
                }
                break;
            case characterStates.inSpace: {
					Debug.Log ("In Space");
                    if (Input.GetAxis("Updown") > 0.000001) {
                        state = characterStates.flying;
                    }
                }
                break;
            case characterStates.landing: {
                    // Landing
                    Debug.Log("Landing");
					if (phase == landingPhases.preparing) {
						Debug.Log ("Preparing");
						// Character already stopped in this frame
						// All informations we need for landing are already in hitInfo
						Vector3 o = ((RaycastHit)hitInfo).transform.position;
//						Vector3 p = cam.transform.position;
						Vector3 p = transform.position;
						Vector3 po = o - p;
						// Debug for checking po direction
//						Debug.DrawRay (cam.transform.position, po * 1000f, Color.white);
						Debug.DrawRay (transform.position, po * 1000f, Color.white);

//						Vector3 directionDown = Camera.main.transform.up * (-1);
						Vector3 directionDown = transform.up * (-1);
						targetRotation = Quaternion.FromToRotation(directionDown, po);
						targetPosition = o;
						landingDirection = po;
						phase = landingPhases.turing;
					}
					else if (phase == landingPhases.turing) {
						if (transform.rotation == targetRotation) {
							Debug.Log ("Finish turning");
//							targetRotation = Quaternion.identity;
							phase = landingPhases.getingDown;
						} else {
							Debug.Log ("turning");
							transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime * 0.5f);
						}
					}
					else if (phase == landingPhases.getingDown) {
						Debug.Log ("Geting down");
//						height = getHeightToSurface ();
//						if (height > offsetHeight) {
//							Debug.Log ("Moving down : height = " + height);
////							moveDirection = (targetPosition - transform.position).normalized;
//							moveDirection = Camera.main.transform.up * (-1);
//							moveDirection *= speed * 0.5f;
//							transform.Translate (moveDirection * Time.deltaTime);
//						} else {
//							phase = landingPhases.notLanding;
//							state = characterStates.onOrbit;
//							Debug.Log ("Finish landing");
//						}

//						moveDirection = transform.TransformDirection (transform.up);
						Vector3 dir = transform.up * (-1);
						Debug.DrawRay (transform.position, dir, Color.green);
//						moveDirection *= (speed) * 0.5f;
//						transform.Translate (moveDirection * Time.deltaTime);
						dir *= 0.5f;
						transform.Translate (dir * Time.deltaTime);
					}

//                    height = getHeightToSurface();
//                    if (height <= offsetHeight) {
//                        state = characterStates.onOrbit;
//                    } else {
//                        //Vector3 down = Vector3.up * (-1);
//                        moveDirection = transform.TransformDirection(Vector3.up);
//                        moveDirection *= speed;
//                        transform.Translate(moveDirection * Time.deltaTime);
//                    }

                }
                break;
            case characterStates.onOrbit: {
					Debug.Log ("OnOrbit");
                    if (Mathf.Abs(Input.GetAxis("Updown")) > 0.1) { // flying_condition to change
                        state = characterStates.flying;
                    }
                    // Walking on orbit
                }
                break;
        }
    }

}
