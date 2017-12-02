using UnityEngine;
using UnityEngine.VR;

public static class VRModeDetector {

    public static bool isInVR;

	// Use this for initialization
    static VRModeDetector () {

		isInVR = Vector3.Distance(InputTracking.GetLocalPosition(VRNode.LeftEye), InputTracking.GetLocalPosition(VRNode.RightEye)) > 0f;
		//isInVR = true;
        Debug.LogWarning("VRMode: " + isInVR);
	}
	
}
