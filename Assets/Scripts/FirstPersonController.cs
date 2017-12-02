using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
namespace UnityStandardAssets.Characters.FirstPerson {
    public class FirstPersonController : MonoBehaviour {
        public Camera cam;
        public MouseLook mouseLook = new MouseLook();

        private void Start() {
            mouseLook.Init(transform, cam.transform);

        }

        private void Update() {
            RotateView();
        }

        private void RotateView() {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            //float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

        }
    }
}