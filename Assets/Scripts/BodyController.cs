﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class BodyController : MonoBehaviour {
    public GameObject head;
    private Vector3 offset;

    // Use this for initialization
    void Start() {
        offset = transform.position - head.transform.position;
        // offset = Vector3.zero;
        Debug.Log(offset);
    }

    void LateUpdate() {
        transform.position = head.transform.position + offset;
        transform.rotation = head.transform.rotation;
    }
}
