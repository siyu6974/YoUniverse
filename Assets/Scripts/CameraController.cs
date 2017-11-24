using System.Collections;
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
    enum landingPhases { notLanding = 0, preparing = 1, turing = 2, getingDown = 3 };
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
        if (rayCasted) {
            targetStar = hit.transform;
            hitInfo = hit;
        }
        else
            hitInfo = null;
    }

    float getHeightToSurface() {
        float distance = -1.0f;
        Ray landingRay = new Ray(transform.position, transform.up * (-1));
        Debug.DrawRay(transform.position, landingRay.direction * 1000f, Color.yellow);

        RaycastHit landingHit;
        bool rayCasted = Physics.Raycast(landingRay, out landingHit, cam.farClipPlane, layerMask);
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
		Vector3 pos, d;
		bool flag = false;

		useRay(transform.forward);
		if (hitInfo != null) {
			pos = ((RaycastHit)hitInfo).transform.position;
			d = pos - transform.position;
			if (d.magnitude <= distanceForLanding) {
				flag = true;
			}
		}
//        if (hitInfo != null && ((RaycastHit)hitInfo).distance <= distanceForLanding)
//            return true;
//        else
//            return false;
		return flag;
    }


    Quaternion turningStart;
    float turningTimer;
    void changeCharacterState() {
        switch (state) {
            case characterStates.flying: {
                    Debug.Log("Flying");
				if (!isFlying()) {
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
                    Debug.Log("In Space");
				if (isFlying()) {
                        state = characterStates.flying;
                    }
                }
                break;
            case characterStates.landing: {
                    // Landing
                    Debug.Log("Landing");
                    if (phase == landingPhases.preparing) {
                        Debug.Log("Preparing");
                        // Character already stopped in this frame
                        // All informations we need for landing are already in hitInfo
                        Vector3 o = ((RaycastHit)hitInfo).transform.position;
                        Vector3 p = transform.position;
                        Vector3 po = o - p;
                        // Debug for checking po direction
                        Debug.DrawRay(transform.position, po * 1000f, Color.white);

                        Vector3 directionDown = transform.up * (-1);
                        targetRotation = Quaternion.FromToRotation(directionDown, po);
                        targetPosition = o;
                        landingDirection = po;
                        phase = landingPhases.turing;
                        turningStart = transform.rotation;
                        turningTimer = 0;

                    } else if (phase == landingPhases.turing) {
                        if (transform.rotation == targetRotation) {
                            Debug.Log("Finish turning");
                            phase = landingPhases.getingDown;
                        } else {
                            Debug.Log("turning");
                            turningTimer += Time.fixedDeltaTime;
                            transform.rotation = Quaternion.Slerp(turningStart, targetRotation, turningTimer);
                        }
                    } else if (phase == landingPhases.getingDown) {
                        Debug.Log("Geting down");
                        height = getHeightToSurface ();
                        if (height >= offsetHeight) {
                        	Debug.Log ("Moving down : height = " + height);
							Vector3 dir = transform.up * (-1);
							dir *= 0.5f;
							transform.Translate(dir * Time.deltaTime, Space.World);
                        } else {
                        	phase = landingPhases.notLanding;
                        	state = characterStates.onOrbit;
                            if (VRModeDetector.isInVR) {
                                orbitEntryPoint = InputTracking.GetLocalPosition(VRNode.CenterEye);
                            } else {
                                orbitEntryPoint = GameObject.Find("TestTrackingObj").transform.position;
                            }
                        	Debug.Log ("Finish landing");
                        }

                        //						moveDirection = transform.TransformDirection (transform.up);

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
                    Debug.Log("OnOrbit");
				if (isFlying()) { // flying_condition to change
                        state = characterStates.flying;
                    }
                    // Walking on orbit
                    //if (targetPosition != Vector3.zero) {
                    //	Vector3 dirRef = targetPosition - transform.position;
                    //	Quaternion rot = Quaternion.FromToRotation(transform.up * (-1), dirRef);
                    //	transform.Rotate (rot.eulerAngles);
                    //}
                    if (orbitEntryPoint != null) {
                        Vector3 entryPoint = (Vector3)orbitEntryPoint;
                        Vector3 currentPos;
                        if (VRModeDetector.isInVR) {
                            currentPos = InputTracking.GetLocalPosition(VRNode.CenterEye);
                        } else {
                            currentPos = GameObject.Find("TestTrackingObj").transform.position;
                        }
                        float theta = (currentPos.x - entryPoint.x) * (2 * Mathf.PI / 3f);
                        float phi = (currentPos.z - entryPoint.z) * (2 * Mathf.PI / 3f);

                        Vector3 transformedPos = new Vector3();
                        //transformedPos.x = 3*Mathf.Sin(currentPos.x - entryPoint.x);
                        //transformedPos.z = 3*Mathf.Sin(currentPos.z - entryPoint.z);
                        transformedPos.x = offsetHeight * Mathf.Sin(theta) * Mathf.Cos(phi);
                        transformedPos.y = offsetHeight * Mathf.Sin(theta) * Mathf.Sin(phi);
                        transformedPos.z = -offsetHeight * Mathf.Cos(theta);

                        transform.position = transformedPos + targetStar.position;
                        transform.up = (transform.position - targetStar.position).normalized;
                        //transform.LookAt(targetStar.position);
                    }

                }
                break;
        }
    }

	bool isFlying() {
		Debug.Log( Input.GetButton ("Left Controller Trigger (Touch)") && Input.GetButton ("Right Controller Trigger (Touch)")
			&& Input.GetButton ("Left Controller Trackpad (Press)") && Input.GetButton ("Right Controller Trackpad (Press)"));
		return Input.GetButton ("Left Controller Trigger (Touch)") && Input.GetButton ("Right Controller Trigger (Touch)")
			&& Input.GetButton ("Left Controller Trackpad (Press)") && Input.GetButton ("Right Controller Trackpad (Press)");
	}

    // real world tracking position just after landing / beginning of orbiting
    Vector3? orbitEntryPoint;
    Transform targetStar;
    
}
