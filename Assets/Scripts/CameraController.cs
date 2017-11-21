using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class CameraController : MonoBehaviour {
    float speed = 2.0f;
    float height;
    float offsetHeight = 1.0f;
    float flyPreparationHeight = 2.0f;
    float distanceForLanding = 5.0f;
    Vector3 moveDirection = Vector3.zero;

    enum characterStates { flying = 1, inSpace = 2, landing = 3, onOrbit = 4 };
    characterStates state;

    RaycastHit hitInfo;
    Ray ray;
    bool rayCasted;

    public Camera cam;

    // Use this for initialization
    void Start() {
        state = characterStates.onOrbit;
    }

    void Update() {
        changeCharacterState();
    }

    void useRay(Vector3 direction) {
        ray = new Ray(transform.position, cam.transform.forward);
        Debug.DrawRay(transform.position, ray.direction * 1000f, Color.red);
        rayCasted = Physics.Raycast(ray, out hitInfo);
    }

    float getHeightToSurface() {
        float distance = -1.0f;
        Vector3 localDown = -transform.TransformDirection(Vector3.up);
        useRay(localDown);
        if (rayCasted) {
            distance = hitInfo.distance;
        }
        return distance;
    }

    bool detectToLand() {
        useRay(Camera.main.transform.forward);
        Debug.Log(hitInfo.distance);
        if (hitInfo.distance <= distanceForLanding)
            return true;
        else
            return false;
    }

    void changeCharacterState() {
        switch (state) {
            case characterStates.flying: {
                    if (Input.GetAxis("Updown") <= 0.000001) {
                        state = characterStates.inSpace;
                    } else {
                        height = getHeightToSurface();
                        //if (height < flyPreparationHeight) {
                        //	moveDirection = new Vector3 (0, Input.GetAxis ("Updown"), 0);
                        //	moveDirection = transform.TransformDirection (moveDirection);
                        //	moveDirection *= speed;
                        //	transform.Translate (moveDirection * Time.deltaTime);
                        //} else {
                        moveDirection = Camera.main.transform.forward;
                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime);
                        //}
                        bool toLand = detectToLand();
                        if (toLand) {
                            // Character already stopped in this frame
                            // All informations we need for landing are already in hitInfo
                            if (hitInfo.transform == null) break;
                            Vector3 o = hitInfo.transform.position;
                            Vector3 p = hitInfo.point;
                            Vector3 op = p - o;
                            // Rotate character for landing
                            Vector3 localUp = transform.TransformDirection(Vector3.up);
                            transform.rotation = Quaternion.FromToRotation(localUp, op);
                            state = characterStates.landing;
                        }
                    }
                }
                break;
            case characterStates.inSpace: {
                    if (Input.GetAxis("Updown") > 0.000001) {
                        state = characterStates.flying;
                    }
                }
                break;
            case characterStates.landing: {
                    // Landing
                    height = getHeightToSurface();
                    if (height <= offsetHeight) {
                        state = characterStates.onOrbit;
                    } else {
                        Vector3 down = Vector3.up * (-1);
                        moveDirection = transform.TransformDirection(down);
                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime);
                    }
                }
                break;
            case characterStates.onOrbit: {
                    if (Input.GetAxis("Updown") <= 0.000001) { // flying_condition to change
                        state = characterStates.flying;
                    }
                    // Walking on orbit
                }
                break;
        }
    }

}
