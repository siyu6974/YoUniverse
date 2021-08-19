using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomController : MonoBehaviour {
    float rate = 500.0f;
    bool zoomed = false;
    Vector3 initPos = Vector3.zero;
    Vector3 aimedPos = Vector3.zero;

    public GameObject bodyPivot;
    public StarGenerator sg;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
		if (Input.GetKeyDown(KeyCode.LeftControl) && zoomed == false) {
            Debug.Log("Zoom in");
            initPos = bodyPivot.transform.position;
            zoomed = true;
            sg.ignoreMovement = true;
        } else if (Input.GetKeyUp(KeyCode.LeftControl) && zoomed == true) {
            Debug.Log("Zoom out");
            bodyPivot.transform.position = initPos;
            zoomed = false;
            sg.ignoreMovement = false;
        }

        if (zoomed) {
            aimedPos = initPos + Camera.main.transform.forward * rate;
            bodyPivot.transform.position = aimedPos;
        }
    }
}
