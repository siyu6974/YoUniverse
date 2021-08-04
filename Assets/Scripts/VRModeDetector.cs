using UnityEngine;
using UnityEngine.VR;

public static class VRModeDetector {

    public static bool isInVR;

	// Use this for initialization
    static VRModeDetector () {

		isInVR = Vector3.Distance(UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye), UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye)) > 0f;
		//isInVR = true;
        Debug.LogWarning("VRMode: " + isInVR);
	}
	
}
