using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class NonVRDebugTool : MonoBehaviour {
    public Camera cam;
    public GameObject body;

    public Transform debugTracking;
    private Vector3? trackingOffset;

    public MouseLook mouseLook = new MouseLook();


    private void Start() {
        mouseLook.Init(transform, cam.transform);
    }


    private void Update() {
        RotateView();
        if (trackingOffset != null)
            cam.transform.position = debugTracking.position - (Vector3)trackingOffset;
    }


    private void RotateView() {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        mouseLook.LookRotation(transform, cam.transform);
        body.transform.rotation = cam.transform.rotation;
    }
}
