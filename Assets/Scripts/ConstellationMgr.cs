using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


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
        if (userConstellationDataSet.Count == 0 && drawMode == 2) {
            drawMode = 0;
        }
    }


    [HideInInspector]
    public ConstellationData[] constellationDataSet;

    [HideInInspector]
    public List<ConstellationData> userConstellationDataSet = new List<ConstellationData>();

    private StarData[] starDataSet;

    public GameObject linePrefab;
    private List<LineRenderer> linesDrawn;

	[HideInInspector]
	public ConstellationData? selected { get; private set;}


	void Start () {
        load_data();
        load_user_data();
        linesDrawn = new List<LineRenderer>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab) || (VRModeDetector.isInVR && Input.GetButtonDown("LMenu"))) {
            toggleDrawMode();
        }
        if (drawMode == 0) return;

        drawAll();
	}

    [SerializeField]
    private TextAsset dataSource;
    [SerializeField]
    private TextAsset constellationNameAbbrSource; // abbr to name

    void load_data() {
        Dictionary<string, string> nameAbbrDict = new Dictionary<string, string>();
        string[] lines = constellationNameAbbrSource.text.Split('\n');
        for (int i = 0; i < lines.Length; i++) {
            string[] components = lines[i].Split(',');
            nameAbbrDict.Add(components[1], components[0]);
        }

        lines = dataSource.text.Split('\n');
        constellationDataSet = new ConstellationData[lines.Length];

        for (int i = 0; i < lines.Length; i++) {
            string[] components = lines[i].Split(' ');
            if (components.Length < 3) continue;
            int nbLink = int.Parse(components[1]);
            int[,] links = new int[nbLink, 2];
            for (int j = 0; j < nbLink; j++) {
                links[j, 0] = int.Parse(components[3 + j * 2]);
                links[j, 1] = int.Parse(components[3 + j * 2 + 1]);
            }
            ConstellationData data = new ConstellationData {
                name = "",
                abbr = components[0],
                links = links
            };
            string tmp;
            if (nameAbbrDict.TryGetValue(data.abbr, out tmp)) 
                data.name = tmp;
            
            constellationDataSet[i] = data; 
        }
        constellationNameAbbrSource = null;
        dataSource = null;
    }

    void load_user_data() {
        using (StreamReader sr = new StreamReader(MyConstants.UserConstellationDataPath)) {
            string[] lines = sr.ReadToEnd().Split('\n');
            
            if (lines.Length == 0) {return;}  
            for (int i = 0; i < lines.Length; i++) {
                string[] components = lines[i].Split(' ');
                if (components.Length < 3) continue;
                int nbLink = int.Parse(components[1]);
                int[,] links = new int[nbLink, 2];
                for (int j = 0; j < nbLink; j++) {
                                    Debug.Log(components);

                    links[j, 0] = int.Parse(components[3 + j * 2]);
                    links[j, 1] = int.Parse(components[3 + j * 2 + 1]);
                }
                ConstellationData data = new ConstellationData {
                    name = components[0],
                    abbr = components[0],
                    links = links
                };
                Debug.Log(data);
                userConstellationDataSet.Add(data); 
            }
        }
    }

    private int lineIndex;
    private void drawAll() {
        lineIndex = 0;

        if (drawMode == 1) {
            // if constellationData not ready, skip this frame
            if (constellationDataSet == null) return;

            for (int i = 0; i < constellationDataSet.Length; i++) {
                ConstellationData c = constellationDataSet[i];
                drawConstellation(c);
            }
        } else if (drawMode == 2) {
            foreach (ConstellationData c in userConstellationDataSet) {
                drawConstellation(c);
            }
        }
    }


//    private void drawConstellation(ConstellationData c) {
	public void drawConstellation(ConstellationData c) {
        for (int j = 0; j < c.links.GetLength(0); j++) {
            Vector3 a = getStarDrawnPosition(c.links[j, 0]);
            Vector3 b = getStarDrawnPosition(c.links[j, 1]);
            if (a != Vector3.zero && b != Vector3.zero)
                drawLine(a, b, Color.white);
        }
    }

	public Vector3 getConstellationCenter(ConstellationData c) {
        Vector3 v;
        long x = 0, y = 0, z = 0;
		int i;
		for (i = 0; i < c.links.GetLength(0); i++) {
            v = getStarRealPosition(c.links[i, 0]);
            x = (long)v.x;
            y = (long)v.y;
            z = (long)v.z;
		}
        v = getStarRealPosition(c.links [i-1, 1]);
		i++;
        x = (long)v.x;
        y = (long)v.y;
        z = (long)v.z;

        return new Vector3(x/i,y/i,z/i);
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


    public string drawConstellationOfSelectedStar(int HIP) {
        clearDrawing();
        for (int i = 0; i < constellationDataSet.Length; i++) {
            ConstellationData c = constellationDataSet[i];

            for (int j = 0; j < c.links.GetLength(0); j++) {
                if (c.links[j, 0] == HIP || c.links[j, 1] == HIP) {
                    drawConstellation(c);
					selected = c;
                    return c.name;
                }
            }
        }
		selected = null;
        return "";
    }


    private Vector3 getStarDrawnPosition(int id) {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        for (int i = 0; i < starDataSet.Length; i++) {
            if (id == starDataSet[i].HIP) {
                return starDataSet[i].drawnPos;
            }
        }
        Debug.LogWarning("zero pos");
        return Vector3.zero;
    }


    private Vector3 getStarRealPosition(int id) {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        for (int i = 0; i < starDataSet.Length; i++) {
            if (id == starDataSet[i].HIP) {
                return starDataSet[i].coord;
            }
        }
        Debug.LogWarning("zero pos");
        return Vector3.zero;
    }

    private void clearDrawing() {
        foreach (LineRenderer lr in linesDrawn) {
            Destroy(lr.gameObject);
        }
        linesDrawn.Clear();
        lineIndex = 0;
    }


    public void clearDrawingWithFadeOut(float duration = 1.3f) {
        foreach (LineRenderer lr in linesDrawn) {
            StartCoroutine(fadeOut(lr, duration));
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
