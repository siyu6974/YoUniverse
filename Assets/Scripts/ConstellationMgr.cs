using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ConstellationData {
    public string name;
    public string abbr;
    public int[,] links;
}

public class ConstellationMgr : MonoBehaviour {
    bool _drawLineEnabled;

    [SerializeField]
    public bool drawLineEnabled {
        get { return _drawLineEnabled; }
        set {
            if (value == false) {
                foreach (GameObject go in linesDrawn) {
                    Destroy(go);
                }
                linesDrawn.Clear();
            }
            _drawLineEnabled = value;
        }
    }

    [HideInInspector]
    public ConstellationData[] contellationDataSet;

    private StarData[] starDataSet;

    public Transform linesParentObjTransform; // parent object for all drawn lines
    public Material lineMaterial;
    private List<GameObject> linesDrawn;


	void Start () {
        //drawLineEnabled = true;
        load_data();
        linesDrawn = new List<GameObject>();
	}
	
	void Update () {
        if (!drawLineEnabled) return;

        foreach (GameObject go in linesDrawn) {
            Destroy(go);
        }
        linesDrawn.Clear();

        if (contellationDataSet == null) return;

        for (int i = 0; i < contellationDataSet.Length;i++) {
            ConstellationData c = contellationDataSet[i];
            for (int j = 0; j < c.links.GetLength(0); j++) {
                Vector3 a = getStar(c.links[j, 0]);
                Vector3 b = getStar(c.links[j, 1]);
                if (a != Vector3.zero && b!=Vector3.zero)
                    drawLine(a, b, Color.white);
            }
        }
        
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


    void drawLine(Vector3 start, Vector3 end, Color color) {
        GameObject myLine = new GameObject();
        myLine.transform.parent = linesParentObjTransform.transform;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = lr.startColor;
        lr.startWidth = .4f;
        lr.endWidth = lr.startWidth;

        // leave a margin 
        Vector3 margin = (end - start).normalized * 10f;
        lr.SetPosition(0, start + margin);
        lr.SetPosition(1, end - margin);

        linesDrawn.Add(myLine);
    }



    private Vector3 getStar(int id) {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        for (int i = 0; i < starDataSet.Length; i++) {
            if (id == starDataSet[i].HIP) {
                return starDataSet[i].drawnPos;
            }
        }
        return Vector3.zero;
    }
}
