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


    void Start() {
    }


    private Vector3 starDrawingPos;

    void Update() {
        if (isCreating && lr != null) {
            Vector3 ray = Input.mousePosition;
            ray.z = Camera.main.farClipPlane * 0.9f;
            ray = Camera.main.ScreenToWorldPoint(ray).normalized;

            drawLine(starDrawingPos, ray * Camera.main.farClipPlane * 0.9f);
        } 

        if (isCreating && Input.GetKeyDown(KeyCode.O)) {
            isCreating = false; // done creating 
            saveConstellation(tmpConstellation);
        }
    }

    void drawLine(Vector3 start, Vector3 end) {
        // leave a margin 
        Vector3 margin = (end - start).normalized * 1f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);
        lr.enabled = true;
    }


    public void constructConstellation(StarData star) {
        int starHIP = star.HIP;
        if (Input.GetMouseButtonDown(1)) {
            if (tmpStarPair[0] == -1) {
                isCreating = true;
                lr = Instantiate(lrPrefab, transform);
                linesDrawn.Add(lr);
                starDrawingPos = star.drawnPos;
                tmpStarPair[0] = starHIP;
            }
        } else if (Input.GetMouseButtonUp(1) && tmpStarPair[0] != -1) {
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

}
