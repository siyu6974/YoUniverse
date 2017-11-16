using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : MonoBehaviour {
    private StarData[] starDataSet;
    public Text starInfoText;
    private int starsMax;
    private LineRenderer lr;

    void Start() {
        StarGenerator sg = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>();
        starDataSet = sg.starDataSet;
        starsMax = sg.starsMax;
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

        if (Input.GetMouseButton(0)) {
            Vector3 ray = Input.mousePosition;
            ray.z = 10f;
            ray = Camera.main.ScreenToWorldPoint(ray).normalized;

            drawLine(transform.position, ray * Camera.main.farClipPlane * 0.9f);
            lr.enabled = true;

            for (int i = 0; i < starsMax; i++) {
                if (Vector3.Angle(starDataSet[i].drawnPos - transform.position, ray) < 1f) {
                    showStarInfo(starDataSet[i]);
                    break;
                }
                // if no star is found, disable the text label
                starInfoText.enabled = false;
            }
        } else if (Input.GetMouseButtonUp(0)) {
            lr.enabled = false;
            starInfoText.enabled = false;
        }
    }

    void drawLine(Vector3 start, Vector3 end) {
        // leave a margin 
        Vector3 margin = (end - start).normalized * 1f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);
    }

    void showStarInfo(StarData star) {
        Vector3 starDrawPosition = star.drawnPos;
        starInfoText.rectTransform.position = Camera.main.WorldToScreenPoint(starDrawPosition) + new Vector3(3f, 3f, 1f);
        string info = "HIP: " + star.HIP + "\n";
        if (star.ProperName != "")
            info += "Name: " + star.ProperName;
        starInfoText.text = info;
        starInfoText.enabled = true;
    }
}
