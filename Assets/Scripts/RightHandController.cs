using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class RightHandController : MonoBehaviour {
    Vector3 offset;
    Vector3 trackingOffset;

    public Transform pivot;

    // Use this for initialization
    void Start() {
        trackingOffset = InputTracking.GetLocalPosition(VRNode.RightHand);
        offset = new Vector3(.50f, 0f, .35f);
    }

    // Update is called once per frame
    void Update() {
        bool vrCtr = VRModeDetector.isInVR;

        if (vrCtr) {
            transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) - trackingOffset + transform.InverseTransformVector(offset);
            transform.rotation = pivot.rotation * InputTracking.GetLocalRotation(VRNode.RightHand);
        }
    }
}
