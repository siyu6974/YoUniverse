using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


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
    private List<ConstellationData> userConstellationDataSet;

    private MenuSelector ms;

	[HideInInspector] public bool customCreationMode;

    void Start() {
        ms = GetComponent<MenuSelector>();
		customCreationMode = false;
    }

    private Vector3 startDrawingPos;

    void Update() {
        if (isCreating && lr != null) {
            Transform cam = Camera.main.transform;
			Vector3 ray = cam.position + cam.forward * 800f;
            drawLine(startDrawingPos, ray);
        }
        //if (Input.GetButtonDown("RMenu")) {
        //    if (ms.enabled) {
        //        ms.enabled = false;
        //        ms.hideMenu();
        //    } else {
        //        ms.enabled = true;
        //        ms.showMenu();
        //    }
        //}
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
		if (Input.GetButtonDown("Fire2") && customCreationMode) {
            if (tmpStarPair[0] == -1) {
                isCreating = true;
                lr = Instantiate(lrPrefab, transform);
                linesDrawn.Add(lr);
                startDrawingPos = star.drawnPos;
                tmpStarPair[0] = starHIP;
            }
		} else if (Input.GetButtonUp("Fire2") && customCreationMode && tmpStarPair[0] != -1) {
            drawLine(startDrawingPos, star.drawnPos); // leave this segment with a correct line
            lr = null;
            tmpStarPair[1] = starHIP;
            if (tmpConstellation == null) {
                tmpConstellation = new CustomConstellation();
            }
            tmpConstellation.links.Add(tmpStarPair);
            tmpStarPair = new int[2] { -1, -1 };
            if (userConstellationDataSet == null) {
                userConstellationDataSet = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationMgr>().userConstellationDataSet;
            }
        }
    }


    private void discardDrawing(CustomConstellation cc) {
        Debug.Log("Discard");
        foreach (LineRenderer l in linesDrawn) {
            Destroy(l.gameObject);
        }
        linesDrawn.Clear();
        tmpStarPair = new int[2] { -1, -1 };
        cc = null;
    }

    private void saveDrawing(CustomConstellation cc) {
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
        userConstellationDataSet.Add(c);
        foreach (LineRenderer l in linesDrawn) {
            Destroy(l.gameObject);
        }
        linesDrawn.Clear();
        tmpStarPair = new int[2] { -1, -1 };

        // save to file
        var p = Application.persistentDataPath + MyConstants.UserConstellationDataFileName;
        StreamWriter writer = new StreamWriter(p, false);
        foreach (ConstellationData con in userConstellationDataSet) {
            string line = $"{con.name} {nbLink}  ";
            for (int i = 0; i < con.links.GetLength(0); i++) {
                line += $"{con.links[i, 0]} {con.links[i, 1]} ";
            }
            writer.WriteLine(line);
        }
        writer.Close();
        Debug.Log("Saved to file " + p);
        cc = null;
    }

    public void discardDrawing() {
        if (!isCreating) return;
        isCreating = false;

        discardDrawing(tmpConstellation);
    }

    public void saveDrawing(string name) {
        if (!isCreating) return;
        isCreating = false;
        tmpConstellation.name = name;
        saveDrawing(tmpConstellation);
        tmpConstellation = null;
    }
}

