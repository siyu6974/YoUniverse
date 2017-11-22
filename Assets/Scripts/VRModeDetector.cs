using UnityEngine;
using UnityEngine.VR;

public static class VRModeDetector {

    public static bool isInVR;

	// Use this for initialization
    static VRModeDetector () {
        isInVR = InputTracking.GetLocalPosition(VRNode.LeftHand) != Vector3.zero;
	}
	
}
