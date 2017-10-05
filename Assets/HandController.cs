using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {
    public Camera cam;
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        RaycastHit hit;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(transform.position, ray.direction * 1000f, Color.red);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Transform objectHit = hit.transform;
            Debug.Log("hit");
            // Do something with the object that was hit by the raycast.
        }
    }
}
