using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGenerator : MonoBehaviour {
    // TODO: https://www.youtube.com/watch?v=-E32N-0qwVM&t=279s
    [SerializeField]
    private TextAsset starCSV;
    private struct StarData {
        public int HIP;
        public string BayerFlamsteed;
        public string ProperName;
        public float Distance;
        public float Mag;
        public float AbsMag;
        public string Spectrum;
        public Color Color;
        public float X, Y, Z;
    }
    private StarData[] starDataSet;
    public ParticleSystem ps;
    private ParticleSystem.Particle[] starParticles;

    public int starsMax = 100;
    public float starSize = 1;

    // Use this for initialization
    void Start() {
        load_data();
        createStars();
    }


    private void createStars() {
        starParticles = new ParticleSystem.Particle[starsMax];
        string[] lines = starCSV.text.Split('\n');
        for (int i = 0; i < starsMax; i++) {
            string[] components = lines[i].Split(',');
            //particleStars[i].position = Random.insideUnitSphere * 10f;
            //points[i].position = Random.insideUnitSphere * 10;
            starParticles[i].position = new Vector3(starDataSet[i].X, starDataSet[i].Z, starDataSet[i].Y).normalized * Camera.main.farClipPlane * 0.9f;

            //points[i].startLifetime = 5000;
            //particleStars[i].remainingLifetime = Mathf.Infinity;

            //TODO: Dark stars are too dark to be visible
            //UNDONE: Add no mesh collider only prefab for raycasting
            //Debug.Log(components[9]);

            starParticles[i].startColor = starDataSet[i].Color * (1.0f - (starDataSet[i].Mag + 1.44f) / 8) * 3;
            
            Debug.Log(starParticles[i].startColor);
            starParticles[i].startSize = starSize;

        }
        ps.SetParticles(starParticles, starParticles.Length);
    }


    // BV < -0.4,+2.0 >
    // Returns Color with RGB <0,1>
    // https://stackoverflow.com/questions/21977786/star-b-v-color-index-to-apparent-rgb-color
    private Color getColor(float bv) {
        float t, r = 0.0f, g = 0.0f, b = 0.0f;
        if (bv < -0.4) bv = -0.4f; if (bv > 2.0) bv = 2.0f;
            if ((bv >= -.4f) && (bv < .0f)) { t = (bv + .4f) / (.0f + .4f); r = 0.61f + (0.11f * t) + (0.1f * t * t); } else if ((bv >= .0f) && (bv < .4f)) { t = (bv - .0f) / (.4f - .0f); r = 0.83f + (0.17f * t); } else if ((bv >= .4f) && (bv < 2.10f)) { t = (bv - .4f) / (2.10f - .4f); r = 1.00f; }
            if ((bv >= -.4f) && (bv < .0f)) { t = (bv + .4f) / (.0f + .4f); g = 0.70f + (0.07f * t) + (0.1f * t * t); } else if ((bv >= .0f) && (bv < .4f)) { t = (bv - .0f) / (.4f - .0f); g = 0.87f + (0.11f * t); } else if ((bv >= .4f) && (bv < 1.60f)) { t = (bv - .4f) / (1.60f - .4f); g = 0.98f - (0.16f * t); } else if ((bv >= 1.60f) && (bv < 2.00f)) { t = (bv - 1.60f) / (2.00f - 1.60f); g = 0.82f - (0.5f * t * t); }
            if ((bv >= -.4f) && (bv < .4f)) { t = (bv + .4f) / (.4f + .4f); b = 1.00f; } else if ((bv >= .4f) && (bv < 1.5f)) { t = (bv - .4f) / (1.5f - .4f); b = 1.00f - (0.47f * t) + (0.1f * t * t); } else if ((bv >= 1.50f) && (bv < 1.94)) { t = (bv - 1.50f) / (1.94f - 1.50f); b = 0.63f - (0.6f * t * t); }
        Vector3 temp = new Vector3(r, g, b).normalized;
        return new Color(temp.x, temp.y, temp.z, 2555555f);
    }


    // Update is called once per frame
    void Update() {

    }

    void load_data() {
        string[] lines = starCSV.text.Split('\n');
        // HIP,BayerFlamsteed,ProperName,Distance,Mag,AbsMag,Spectrum,ColorIndex,X,Y,Z

        starDataSet = new StarData[starsMax];

        for (int i = 0; i < starsMax; i++) {
            string[] components = lines[i].Split(',');
            starDataSet[i].HIP = int.Parse(components[0]);
            starDataSet[i].BayerFlamsteed = components[1];
            starDataSet[i].ProperName = components[2];
            starDataSet[i].Distance = float.Parse(components[3]);
            starDataSet[i].Mag = float.Parse(components[4]);
            starDataSet[i].AbsMag = float.Parse(components[5]);
            starDataSet[i].Spectrum = components[6];
            try {
                starDataSet[i].Color = getColor(float.Parse(components[7]));
            } catch {
                starDataSet[i].Color = Color.white;
            }
            starDataSet[i].X = float.Parse(components[8]);
            starDataSet[i].Y = float.Parse(components[9]);
            starDataSet[i].Z = float.Parse(components[10]);
        }
    }
}
