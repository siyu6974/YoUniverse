using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class RightHandController : MonoBehaviour {
    Vector3 offset;
    public Transform pivot;

    // Use this for initialization
    void Start() {
        offset = InputTracking.GetLocalPosition(VRNode.RightHand) - new Vector3(.501f, 0f, .35f);
    }

    // Update is called once per frame
    void Update() {
        bool vrCtr = VRModeDetector.isInVR;

        if (vrCtr) {
            transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) - offset;
            transform.rotation = pivot.rotation * InputTracking.GetLocalRotation(VRNode.RightHand);
        }
    }
}
