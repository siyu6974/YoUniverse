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
        trackingOffset = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
        offset = new Vector3(.50f, 0f, .35f);
    }

    // Update is called once per frame
    void Update() {
        bool vrCtr = VRModeDetector.isInVR;

        if (vrCtr) {
            transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand) - trackingOffset + transform.InverseTransformVector(offset);
            transform.rotation = pivot.rotation * UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
        }
    }
}
