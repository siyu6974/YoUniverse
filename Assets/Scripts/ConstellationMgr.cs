using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConstellationData {
    public string name;
    public string abbr;
    public int[,] links;
}

public class ConstellationMgr : MonoBehaviour {

    private int drawMode; // 0 disabled, 1 std 88, 2 custom;

    private void toggleDrawMode() {
        clearDrawing();
        drawMode = (drawMode+1) % 3;
        if (userContellationDataSet.Count == 0 && drawMode == 2) {
            drawMode = 0;
        }
    }


    [HideInInspector]
    public ConstellationData[] contellationDataSet;

    [HideInInspector]
    public List<ConstellationData> userContellationDataSet;

    private StarData[] starDataSet;

    public GameObject linePrefab;
    private List<LineRenderer> linesDrawn;


	void Start () {
        load_data();
        userContellationDataSet = new List<ConstellationData>();
        linesDrawn = new List<LineRenderer>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.N) || (VRModeDetector.isInVR && Input.GetButtonDown("LMenu"))) {
            toggleDrawMode();
        }
        if (drawMode == 0) return;

        drawAll();
	}


    public TextAsset dataSource;

    void load_data() {
        string[] lines = dataSource.text.Split('\n');
        contellationDataSet = new ConstellationData[lines.Length];

        for (int i = 0; i < lines.Length; i++) {
            string[] components = lines[i].Split(' ');
            if (components.Length < 3) continue;
            int nbLink = int.Parse(components[1]);
            int[,] links = new int[nbLink, 2];
            for (int j = 0; j < nbLink; j++) {
                links[j, 0] = int.Parse(components[3 + j * 2]);
                links[j, 1] = int.Parse(components[3 + j * 2 + 1]);
            }
            ConstellationData data = new ConstellationData() {
                name = "", // TODO
                abbr = components[0],
                links = links
            };
            contellationDataSet[i] = data; 
        }

        dataSource = null;
    }


    private int lineIndex;
    private void drawAll() {
        lineIndex = 0;

        if (drawMode == 1) {
            // if contellationData not ready, skip this frame
            if (contellationDataSet == null) return;

            for (int i = 0; i < contellationDataSet.Length; i++) {
                ConstellationData c = contellationDataSet[i];
                drawConstellation(c);
            }
        } else if (drawMode == 2) {
            foreach (ConstellationData c in userContellationDataSet) {
                drawConstellation(c);
            }
        }
    }


    private void drawConstellation(ConstellationData c) {
        for (int j = 0; j < c.links.GetLength(0); j++) {
            Vector3 a = getStarDrawnPosition(c.links[j, 0]);
            Vector3 b = getStarDrawnPosition(c.links[j, 1]);
            if (a != Vector3.zero && b != Vector3.zero)
                drawLine(a, b, Color.white);
        }
    }


    void drawLine(Vector3 start, Vector3 end, Color color) {
        LineRenderer lr;
        try {
            // try to reuse line renderer 
            lr = linesDrawn[lineIndex];
        } catch (System.ArgumentOutOfRangeException){
            lr = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
            linesDrawn.Add(lr);
        }
        lr.material.color = color;

        // leave a margin 
        Vector3 margin = (end - start).normalized * 10f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);

        lineIndex++;
    }


    public void drawConstellationOfSelectedStar(int HIP) {
        clearDrawing();
        for (int i = 0; i < contellationDataSet.Length; i++) {
            ConstellationData c = contellationDataSet[i];

            for (int j = 0; j < c.links.GetLength(0); j++) {
                if (c.links[j, 0] == HIP || c.links[j, 1] == HIP) {
                    drawConstellation(c);
                    return;
                }
            }
        }
    }


    private Vector3 getStarDrawnPosition(int id) {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        for (int i = 0; i < starDataSet.Length; i++) {
            if (id == starDataSet[i].HIP) {
                return starDataSet[i].drawnPos;
            }
        }
        return Vector3.zero;
    }


    private void clearDrawing() {
        foreach (LineRenderer lr in linesDrawn) {
            Destroy(lr.gameObject);
        }
        linesDrawn.Clear();
        lineIndex = 0;
    }


    public void clearDrawingWithFadeOut() {
        foreach (LineRenderer lr in linesDrawn) {
            StartCoroutine(fadeOut(lr, 1.3f));
        }
        linesDrawn.Clear();
        lineIndex = 0;
    }


    private IEnumerator fadeOut(LineRenderer lr, float duration) {
        // Execute this loop once per frame until the timer exceeds the duration.
        float timer = 0f;
        Color startCol = lr.material.color;
        Color endCol = startCol;
        endCol.a = 0;

        while (timer <= duration) {
            // Set the colour based on the normalised time.
            lr.material.color = Color.Lerp(startCol, endCol, timer / duration);

            // Increment the timer by the time between frames and return next frame.
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(lr.gameObject);
    }
}
