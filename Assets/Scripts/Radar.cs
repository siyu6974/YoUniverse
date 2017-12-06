﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Radar : MonoBehaviour {
    public GameObject arrowPref;
    public GameObject circlePref;


    StarData[] starDataSet;
    List<Marker> nearStars = new List<Marker>();

    // not using struct because marker need to be changed later
    class Marker {
        public StarData star;
        public bool isOnSight;
        public GameObject marker;
    }

	// Use this for initialization
	void Start () {
        starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;
        //StartCoroutine(test());
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("scan");
            scanEnvironment();
        }
        if (Input.GetMouseButton(1)) {
            showMarker();
        }
	}


    public void scanEnvironment() {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        clearMarker();
        // find stars within 10 light year, but not within 0.1 lr
        IEnumerable<StarData> ns = from s in starDataSet where s.distance < 100 && s.distance > 1 select s;
        if (ns.Count() == 0) {
            StarData nearestStar = starDataSet.Aggregate((minItem, nextItem) => (minItem.distance < nextItem.distance && minItem.distance > 1) ? minItem : nextItem);
            Marker m = new Marker {star = nearestStar};
            nearStars.Add(m);
        } else {
            foreach (StarData s in ns) {
                Marker m = new Marker {star = s};
                nearStars.Add(m);
            }
        }
    }

    public void showMarker() {
        for (int i = 0; i < nearStars.Count(); i++) {
            Marker m = nearStars[i];

            Camera cam = Camera.main;
            Vector3 dir = m.star.drawnPos - cam.transform.position - cam.transform.forward * (cam.farClipPlane * 0.9f);

            Vector3 dirProjectionOnScreen = Vector3.ProjectOnPlane(dir, cam.transform.forward);

            Vector3 starScreenPos = cam.WorldToViewportPoint(m.star.drawnPos);
            if (starScreenPos.z > 0 && starScreenPos.x > 0 && starScreenPos.x < 1 && starScreenPos.y > 0 && starScreenPos.y < 1) {
                // draw circle
                if (!m.isOnSight) {
                    // just get in sight
                    if (m.marker != null) 
                        Destroy(m.marker);
                }
                Vector3 pos = Vector3.ClampMagnitude(m.star.drawnPos - cam.transform.position, 30);
                Debug.DrawRay(cam.transform.position, pos * 1000, Color.red);
                if (m.marker == null) {
                    
                    m.marker = Instantiate(circlePref, cam.transform.position + pos, Quaternion.identity);
                    m.marker.transform.LookAt(cam.transform);
                }
                m.isOnSight = true;

            } else {
                // draw arrow
                Vector3 pos = cam.transform.forward * 10 + Camera.main.transform.position;
                if (m.isOnSight) {
                    // just get out of sight
                    if (m.marker != null)
                        Destroy(m.marker);
                }
                if (m.marker == null) {
                    m.marker = Instantiate(arrowPref, pos, Quaternion.identity);
                }
                m.marker.transform.LookAt(dirProjectionOnScreen, cam.transform.forward * -1);
                m.marker.transform.position = pos + m.marker.transform.forward * 5;
                m.isOnSight = false;

            }

        }

    }


    void clearMarker() {
        foreach (Marker mm in nearStars) {
            Destroy(mm.marker);
        }
        nearStars.Clear();
    }
}
