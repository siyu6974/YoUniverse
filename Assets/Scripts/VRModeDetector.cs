using UnityEngine;
using UnityEngine.VR;

public static class VRModeDetector {

    public static bool isInVR;

	// Use this for initialization
    static VRModeDetector () {

        isInVR = Vector3.Distance(InputTracking.GetLocalPosition(VRNode.LeftHand), InputTracking.GetLocalPosition(VRNode.RightHand)) > 0.0001f;
		//isInVR = true;
        Debug.LogWarning("VRMode: " + isInVR);
	}
	
}
