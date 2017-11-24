using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConstellationCreater : MonoBehaviour {
    private class CustomConstellation {
        public string name = null;
        public string abbr = null;
        public List<int[]> links = new List<int[]>();
    }

    private LineRenderer lr;
    private List<LineRenderer> linesDrawn = new List<LineRenderer>();
    public LineRenderer lrPrefab;

    private bool isCreating = false;
    private int[] tmpStarPair = new int[2]{-1,-1};
    private CustomConstellation tmpConstellation = new CustomConstellation();
    private List<ConstellationData> userContellationDataSet;

	public Transform pointerDirection;

    void Start() {
    }


    private Vector3 startDrawingPos;

    void Update() {
        if (isCreating && lr != null) {
			Vector3 ray = pointerDirection.forward * Camera.main.farClipPlane * 0.9f;
           
            drawLine(startDrawingPos, ray);
        } 
    }

    void drawLine(Vector3 start, Vector3 end) {
        // leave a margin 
        Vector3 margin = (end - start).normalized * 10f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);
        lr.enabled = true;
    }


    public void constructConstellation(StarData star) {
        int starHIP = star.HIP;
		if (Input.GetButtonDown("Fire2")) {
            if (tmpStarPair[0] == -1) {
                isCreating = true;
                lr = Instantiate(lrPrefab, transform);
                linesDrawn.Add(lr);
                startDrawingPos = star.drawnPos;
                tmpStarPair[0] = starHIP;
            }
		} else if (Input.GetButtonUp("Fire2") && tmpStarPair[0] != -1) {
            drawLine(startDrawingPos, star.drawnPos); // leave this segment with a correct line
            lr = null;
            tmpStarPair[1] = starHIP;
            if (tmpConstellation == null) {
                tmpConstellation = new CustomConstellation();
            }
            tmpConstellation.links.Add(tmpStarPair);
            tmpStarPair = new int[2] { -1, -1 };
            if (userContellationDataSet == null) {
                userContellationDataSet = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationMgr>().userContellationDataSet;
            }
        }
    }


    private void saveConstellation(CustomConstellation cc) {
        Debug.Log("save");
        ConstellationData c = new ConstellationData();
        c.name = cc.name;
        c.abbr = cc.abbr;

        int nbLink = cc.links.Count;
        c.links = new int[nbLink, 2];
        for (int i = 0; i < nbLink; i++) {
            c.links[i, 0] = cc.links[i][0];
            c.links[i, 1] = cc.links[i][1];
        }
        userContellationDataSet.Add(c);
        foreach (LineRenderer l in linesDrawn) {
            Destroy(l.gameObject);
        }
        linesDrawn.Clear();
        cc = null;
    }


    public void saveConstellation() {
        if (isCreating) {
            isCreating = false; // done creating 
            saveConstellation(tmpConstellation);
        }
    }
}
