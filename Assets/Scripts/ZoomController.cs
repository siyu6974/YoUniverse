using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class ZoomController : MonoBehaviour {
    float rate = 500.0f;
    bool zoomed = false;
    Vector3 initPos = Vector3.zero;
    Vector3 aimedPos = Vector3.zero;

    public GameObject bodyPivot; // cameras
    public StarGenerator sg;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

        bool vrCtr = VRModeDetector.isInVR;

        bool vrInput = (Vector3.Distance(InputTracking.GetLocalPosition(VRNode.LeftHand), InputTracking.GetLocalPosition(VRNode.LeftEye)) < .3f) &&
            (Vector3.Distance(InputTracking.GetLocalPosition(VRNode.RightHand), InputTracking.GetLocalPosition(VRNode.RightEye)) < .3f);

        if ((vrCtr && vrInput) || Input.GetKeyDown(KeyCode.P) && zoomed == false) {
            Debug.Log("Zoom in");
            initPos = bodyPivot.transform.position;
            zoomed = true;
            sg.ignoreMovement = true;
        } else if (((vrCtr && !vrCtr) || Input.GetKeyUp(KeyCode.P)) && zoomed == true) {
            Debug.Log("Zoom out");
            bodyPivot.transform.position = initPos;
            zoomed = false;
            sg.ignoreMovement = false;
        }

        if (zoomed) {
            aimedPos = initPos + Camera.main.transform.forward * rate;
            bodyPivot.transform.position = aimedPos;
        }
    }
}
