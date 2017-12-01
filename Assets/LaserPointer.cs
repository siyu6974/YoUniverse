﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class LaserPointer : MonoBehaviour {
    private StarData[] starDataSet;
    public Text starInfoText;
    private int starsMax;
    private LineRenderer lr;
    public ConstellationCreater constellationCreater;
    public ConstellationMgr constellationMgr;
    Vector3 ray;

    void Start() {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        if (starDataSet == null) {
            StarGenerator sg = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>();
            starDataSet = sg.starDataSet;
            starsMax = sg.starsMax;
            return;
        }

        if (VRModeDetector.isInVR)
            transform.rotation = InputTracking.GetLocalRotation(VRNode.RightHand);
        //      Debug.Log(Input.GetAxis ("Axis1D.PrimaryIndexTrigger"));
        //      Debug.Log(Input.GetButton("Fire1"));
        if (Input.GetButton("Fire1")) {
            ray = transform.forward * Camera.main.farClipPlane * 0.9f;
            //            ray.z = Camera.main.farClipPlane * 0.9f;
            //            ray = Camera.main.ScreenToWorldPoint(ray).normalized;
            lr.enabled = true;
            for (int i = 0; i < starsMax; i++) {
                if (Vector3.Angle(starDataSet[i].drawnPos, ray) < 1f) {
                    showStarInfo(starDataSet[i]);

                    constellationMgr.drawConstellationOfSelectedStar(starDataSet[i].HIP);

                    constellationCreater.SendMessage("constructConstellation", starDataSet[i]);
                    break;
                }
                // if no star is found, disable the text label
                starInfoText.enabled = false;
            }
        } else if (Input.GetButtonUp("Fire1")) {
            lr.enabled = false;
            starInfoText.enabled = false;

            constellationMgr.clearDrawingWithFadeOut();
        }
    }

    private void LateUpdate()
    {
        if (lr.enabled)
            drawLine(transform.position, ray);
    }

    void drawLine(Vector3 start, Vector3 end) {
        // leave a margin 
        Vector3 margin = (end - start).normalized * .5f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);
    }

    void showStarInfo(StarData star) {
        //Vector3 starDrawPosition = star.drawnPos;
        //        starInfoText.rectTransform.position = Camera.main.WorldToScreenPoint(starDrawPosition) + new Vector3(3f, 3f, 1f);
        string info = "HIP: " + star.HIP + "\n";
        if (star.ProperName != "")
            info += "Name: " + star.ProperName;
        starInfoText.text = info;
        starInfoText.enabled = true;
        //Debug.Log (info);
    }
}
