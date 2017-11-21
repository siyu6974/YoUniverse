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

    RaycastHit? hitInfo;
    Ray ray;

    public Camera cam;
    int layerMask;
    // Use this for initialization
    void Start() {
        state = characterStates.onOrbit;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
    }

    void Update() {
        changeCharacterState();
    }

    void useRay(Vector3 direction) {
        ray = new Ray(cam.transform.position, direction);
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
        Vector3 localDown = -transform.TransformDirection(Vector3.up);
        useRay(localDown);
        //distance = hitInfo?.distance;
        if (hitInfo != null)
            distance = ((RaycastHit)hitInfo).distance;
        return distance;
    }

    bool detectToLand() {
        useRay(Camera.main.transform.forward);
        if (hitInfo != null && ((RaycastHit)hitInfo).distance <= distanceForLanding)
            return true;
        else
            return false;
    }

    void changeCharacterState() {
        switch (state) {
            case characterStates.flying: {
                    if (Mathf.Abs(Input.GetAxis("Updown")) <= 0.000001) {
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
                            Vector3 o = ((RaycastHit)hitInfo).transform.position;
                            Vector3 p = ((RaycastHit)hitInfo).point;
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
                    Debug.Log("landing");
                    height = getHeightToSurface();
                    if (height <= offsetHeight) {
                        state = characterStates.onOrbit;
                    } else {
                        //Vector3 down = Vector3.up * (-1);
                        moveDirection = transform.TransformDirection(Vector3.up);
                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime);
                    }
                }
                break;
            case characterStates.onOrbit: {
                    if (Mathf.Abs(Input.GetAxis("Updown")) <= 0.000001) { // flying_condition to change
                        state = characterStates.flying;
                    }
                    // Walking on orbit
                }
                break;
        }
    }

}
