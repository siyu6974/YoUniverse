using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class NonVRDebugTool : MonoBehaviour {
    public Camera cam;
    public GameObject pivot;
    FlightController fc;

    public Transform debugTracking;
    private Vector3? trackingOffset;

    public MouseLook mouseLook = new MouseLook();


    private void Start() {
        mouseLook.Init(transform, cam.transform);
        mouseLook.MaximumX = 90;
        mouseLook.MinimumX = -90;
        trackingOffset = cam.transform.position - debugTracking.position;
        fc = pivot.GetComponent<FlightController>();
    }


    private void Update() {
        RotateView();
        // if (trackingOffset != null && fc.state != CharacterStates.flying)
        //     cam.transform.localPosition = debugTracking.position - (Vector3)trackingOffset;
    }


    private void RotateView() {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;
        // if (Input.GetButton("Fire1") {

        // }
        mouseLook.LookRotation(transform, cam.transform);
        //pivot.transform.rotation = cam.transform.rotation;
    }
}
