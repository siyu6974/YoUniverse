using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class CameraController : MonoBehaviour {
    const float defaultSpeed = 4f;
	const float landingSpeed = 0.5f;
    float speed;

    float height;
    float offsetHeight = 3.0f;
    
    float distanceForLanding = 5.0f;
    Vector3 moveDirection = Vector3.zero;
    Vector3 targetPosition = Vector3.zero;
    Quaternion targetRotation;
    Vector3 landingDirection;

	Quaternion turningStart;
	float turningTimer;

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
        state = characterStates.inSpace;
        phase = landingPhases.notLanding;
        speed = defaultSpeed;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    void FixedUpdate() {
        // Debug for checking down direction for character
        Vector3 directionDown = transform.up * (-1);
        Debug.DrawRay(transform.position, directionDown * 1000f, Color.blue);

        changeCharacterState();
    }

    void useRay(Vector3 direction) {
        ray = new Ray(transform.position, direction);

        // Debug for checking forward direction for character
        Debug.DrawRay(transform.position, ray.direction * 1000f, Color.red);

        RaycastHit hit;
        bool rayCasted = Physics.Raycast(ray, out hit, cam.farClipPlane, layerMask);
        if (rayCasted) {
            targetSubOrbitPos = hit.point;
            hitInfo = hit;
        } else
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

        if (hitInfo != null)
            distance = ((RaycastHit)hitInfo).distance;
		
        return distance;
    }

    bool detectToLand() {
        Vector3 pos, d;
        bool flag = false;

        useRay(transform.forward);
        if (hitInfo != null) {
            pos = ((RaycastHit)hitInfo).point;
            d = pos - transform.position;
            if (d.magnitude <= distanceForLanding) {
                flag = true;
            }
        }

        return flag;
    }
		
    void changeCharacterState() {
        switch (state) {
            case characterStates.flying: {
                    Debug.Log("Flying");
                    if (!isFlying()) {
                        speed = defaultSpeed;
                        state = characterStates.inSpace;
                    } else {
                        moveDirection = Camera.main.transform.forward;

                        speed += (InputTracking.GetLocalPosition(VRNode.RightHand).y - InputTracking.GetLocalPosition(VRNode.CenterEye).y);

                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime);
                        
                        bool toLand = detectToLand();
                        if (toLand) {
                            speed = defaultSpeed;
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
                        height = getHeightToSurface();
                        if (height >= offsetHeight) {
                            Vector3 dir = transform.up * (-1);
							dir *= landingSpeed;
                            transform.Translate(dir * Time.deltaTime, Space.World);
                        } else {
                            phase = landingPhases.notLanding;
                            state = characterStates.onOrbit;
                            if (VRModeDetector.isInVR) {
                                orbitEntryPoint = InputTracking.GetLocalPosition(VRNode.CenterEye);
                            } else {
                                orbitEntryPoint = GameObject.Find("TestTrackingObj").transform.position;
                            }
                            Debug.Log("Finish landing");
                        }
                    }
                }
                break;
            case characterStates.onOrbit: {
                    Debug.Log("OnOrbit");
                    if (isFlying()) {
                        state = characterStates.flying;
                    }
                    // Walking on orbit
                    if (orbitEntryPoint != null) {
                        Vector3 entryPoint = (Vector3)orbitEntryPoint;
                        Vector3 currentPos;
                        if (VRModeDetector.isInVR) {
                            currentPos = InputTracking.GetLocalPosition(VRNode.CenterEye);
                        } else {
                            currentPos = GameObject.Find("TestTrackingObj").transform.position;
                        }
                        float theta = (currentPos.x - entryPoint.x) * (2 * Mathf.PI / 5f);
                        float phi = (currentPos.z - entryPoint.z) * (2 * Mathf.PI / 5f);

                        Vector3 transformedPos = new Vector3();
                        transformedPos.x = offsetHeight * Mathf.Sin(theta) * Mathf.Cos(phi);
                        transformedPos.y = offsetHeight * Mathf.Sin(theta) * Mathf.Sin(phi);
                        transformedPos.z = -offsetHeight * Mathf.Cos(theta);

                        transform.position = transformedPos + targetSubOrbitPos;
                    }
                }
                break;
        }
    }

    bool isFlying() {
        return Input.GetKey(KeyCode.O) || Input.GetButton("Left Controller Trigger (Touch)")
            && Input.GetButton("Left Controller Trackpad (Press)") && Input.GetButton("Right Controller Trackpad (Press)");
    }

    // real world tracking position just after landing / beginning of orbiting
    Vector3? orbitEntryPoint;
    Vector3 targetSubOrbitPos;

}
