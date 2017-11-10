using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {
    private StarData[] starDataSet;
    private int starsMax;
    private LineRenderer lr;

    void Start() {
        StarGenerator sg = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>();
        starDataSet = sg.starDataSet;
        starsMax = sg.starsMax;
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (starDataSet == null) {
            StarGenerator sg = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>();
            starDataSet = sg.starDataSet;
            starsMax = sg.starsMax;
            return;
        }

        Vector3 ray = Input.mousePosition;
        ray.z = 10f;
        ray = Camera.main.ScreenToWorldPoint(ray).normalized;

        drawLine(transform.position, ray * Camera.main.farClipPlane * 0.9f, Color.red);

        for (int i = 0; i < starsMax; i++) {
            if (Vector3.Angle(starDataSet[i].drawnPos - transform.position, ray) < 1f) {
                if (starDataSet[i].ProperName != "") {
                    Debug.Log(starDataSet[i].ProperName);
                    break;
                }
            }
        }
    }

    void drawLine(Vector3 start, Vector3 end, Color color) {
        // leave a margin 
        Vector3 margin = (end - start).normalized * 1f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);
    }
}
