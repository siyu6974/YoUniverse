using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public struct ConstellationData {
    public string name;
    public string abbr;
    public int[,] links;
}

public class ConstellationMgr : MonoBehaviour {

    enum SkyCulture
    {
        Westen = 1,
        User
    }
    private SkyCulture skyCulture = SkyCulture.Westen; // 0 disabled, 1 std 88, 2 custom;
    private bool shouldDrawAllConstellations = false;


    [HideInInspector]
    public ConstellationData[] constellationDataSet;

    [HideInInspector]
    public List<ConstellationData> userConstellationDataSet = new List<ConstellationData>();

    private StarData[] starDataSet;

    public GameObject linePrefab;
    private List<LineRenderer> linesDrawn;

    private List<GameObject> labelShown;

	[HideInInspector]
	public ConstellationData? selected { get; private set;}

    private Camera _camera;

	void Start () {
        load_data();
        load_user_data();
        linesDrawn = new List<LineRenderer>(500);
        labelShown = new List<GameObject>(100);
        _camera = Camera.main;
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab) || (VRModeDetector.isInVR && Input.GetButtonDown("LMenu"))) {
            shouldDrawAllConstellations = !shouldDrawAllConstellations;
            if (shouldDrawAllConstellations == false) clearDrawing();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { 
            clearDrawing();
            skyCulture = SkyCulture.Westen;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) { 
            clearDrawing();
            skyCulture = SkyCulture.User;
        }
	}


    void LateUpdate () {
        if (skyCulture == 0) return;
        if (shouldDrawAllConstellations) {
            drawAll();
            for (int i=lineIndex; i <linesDrawn.Count; i++) {
                linesDrawn[i].enabled = false;
            }
        }
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
                    links[j, 0] = int.Parse(components[3 + j * 2]);
                    links[j, 1] = int.Parse(components[3 + j * 2 + 1]);
                }
                ConstellationData data = new ConstellationData {
                    name = components[0],
                    abbr = components[0],
                    links = links
                };
                userConstellationDataSet.Add(data); 
            }
        }
    }

    private int lineIndex;
    private int lableIndex;
    private void drawAll() {
        lineIndex = 0;
        lableIndex = 0;

        if (skyCulture == SkyCulture.Westen) {
            // if constellationData not ready, skip this frame
            if (constellationDataSet == null) return;

            for (int i = 0; i < constellationDataSet.Length; i++) {
                ConstellationData c = constellationDataSet[i];
                drawConstellation(c);
            }
        } else if (skyCulture == SkyCulture.User) {
            foreach (ConstellationData c in userConstellationDataSet) {
                drawConstellation(c);
            }
        }
    }


	public void drawConstellation(ConstellationData c) {
        for (int j = 0; j < c.links.GetLength(0); j++) {
            Vector3 a = getStarDrawnPosition(c.links[j, 0]);
            Vector3 b = getStarDrawnPosition(c.links[j, 1]);
            Vector3 a_r = a - _camera.transform.position;

            if (Vector3.Angle(a_r, _camera.transform.forward) > _camera.fieldOfView) {
                // Do not draw if object is off screen
                continue;
            }
            if (a != Vector3.zero && b != Vector3.zero)
                drawLine(a, b, Color.white);
        }
        showConstellationLabel(c);
    }


	public void showConstellationLabel(ConstellationData c) {
        Vector3 position = getConstellationApparentCenter(c);

        GameObject textGO;
        Text text;
        try {
            // try to reuse line renderer 
            textGO = labelShown[lableIndex];
            text = textGO.GetComponent<Text>();
        } catch (System.ArgumentOutOfRangeException) {
            GameObject canvas = GameObject.Find("ConstellationNameCanvas");
            textGO = new GameObject();
            textGO.transform.parent = canvas.transform;
            text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.fontSize = 17;
            textGO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
            labelShown.Add(textGO);
        }
        textGO.name = c.name + "_label";
        text.text = c.name;
        text.alignment = TextAnchor.MiddleCenter;
        textGO.transform.position = position;
        textGO.transform.LookAt(_camera.transform);
        textGO.transform.Rotate(0f, 180f, 0f);
        lableIndex++;
    }


	public Vector3 getConstellationApparentCenter(ConstellationData c) {
        Vector3 v;
        long x = 0, y = 0, z = 0;
        HashSet<int> stars = new HashSet<int>();
        for (int i = 0; i < c.links.GetLength(0); i++) {
            stars.Add(c.links[i, 0]);
            stars.Add(c.links[i, 1]);
        }
		foreach (int s in stars) {
            v = getStarDrawnPosition(s);
            x += (long)v.x;
            y += (long)v.y;
            z += (long)v.z;
		}
        int n = stars.Count;
        return new Vector3(x/n,y/n,z/n);
	}


	public Vector3 getConstellationCenter(ConstellationData c) {
        Vector3 v;
        long x = 0, y = 0, z = 0;
        HashSet<int> stars = new HashSet<int>();
        for (int i = 0; i < c.links.GetLength(0); i++) {
            stars.Add(c.links[i, 0]);
            stars.Add(c.links[i, 1]);
        }
		foreach (int s in stars) {
            v = getStarRealPosition(s);
            x += (long)v.x;
            y += (long)v.y;
            z += (long)v.z;
		}
        int n = stars.Count;
        return new Vector3(x/n,y/n,z/n);
	}


    void drawLine(Vector3 start, Vector3 end, Color color) {
        LineRenderer lr;
        try {
            // try to reuse line renderer 
            lr = linesDrawn[lineIndex];
            lr.enabled = true;
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
        if (shouldDrawAllConstellations) return "";
        clearDrawing();
        if (skyCulture == SkyCulture.Westen) {
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
        } else if (skyCulture == SkyCulture.User) {
            foreach (ConstellationData c in userConstellationDataSet) {
                for (int j = 0; j < c.links.GetLength(0); j++) {
                    if (c.links[j, 0] == HIP || c.links[j, 1] == HIP) {
                        drawConstellation(c);
                        selected = c;
                        return c.name;
                    }
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
        return Vector3.zero;
    }


    private Vector3 getStarRealPosition(int id) {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        for (int i = 0; i < starDataSet.Length; i++) {
            if (id == starDataSet[i].HIP) {
                return starDataSet[i].coord;
            }
        }
        return Vector3.zero;
    }

    private void clearDrawing() {
        foreach (LineRenderer lr in linesDrawn) {
            Destroy(lr.gameObject);
        }
        foreach (GameObject go in labelShown) {
            Destroy(go);
        }
        linesDrawn.Clear();
        labelShown.Clear();
        lineIndex = 0;
        lableIndex = 0;
    }


    public void clearDrawingWithFadeOut(float duration = 1.3f) {
        foreach (LineRenderer lr in linesDrawn) {
            StartCoroutine(fadeOut(lr, duration));
        }
        foreach (GameObject go in labelShown) {
            Destroy(go);
        }
        linesDrawn.Clear();
        labelShown.Clear();
        lineIndex = 0;
        lableIndex = 0;
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
