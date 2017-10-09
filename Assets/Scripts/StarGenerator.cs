using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// HIP,BayerFlamsteed,ProperName,RA,Dec,Distance,Mag,AbsMag,Spectrum,ColorIndex,X,Y,Z
public class StarGenerator : MonoBehaviour {
    // TODO: https://www.youtube.com/watch?v=-E32N-0qwVM&t=279s
    public TextAsset starCSV;

    public ParticleSystem ps;
    private ParticleSystem.Particle[] starParticles;

    public int starsMax = 100;
    public float starSize = 1;

    // Use this for initialization
    void Start() {
        createStars();
    }


    private void createStars() {
        starParticles = new ParticleSystem.Particle[starsMax];
        string[] lines = starCSV.text.Split('\n');
        for (int i = 0; i < starsMax; i++) {
            string[] components = lines[i].Split(',');
            //particleStars[i].position = Random.insideUnitSphere * 10f;
            //points[i].position = Random.insideUnitSphere * 10;
            starParticles[i].position = new Vector3(float.Parse(components[10]),
            float.Parse(components[12]),
            float.Parse(components[11])).normalized * Camera.main.farClipPlane * 0.9f;

            //points[i].startLifetime = 5000;
            //particleStars[i].remainingLifetime = Mathf.Infinity;

            //TODO: Dark stars are too dark to be visible
            //UNDONE: Add no mesh collider only prefab for raycasting
            //Debug.Log(components[9]);

            try {
                //starParticles[i].startColor =Color.white * (1.0f - ((float.Parse(components[6]) + 1.44f) / 8)) *2;

                starParticles[i].startColor = getColor(float.Parse(components[9])) * (1.0f - ((float.Parse(components[6]) + 1.44f) / 8)) * 3;
            } catch (System.FormatException e) {
                starParticles[i].startColor = Color.white * (1.0f - ((float.Parse(components[6]) + 1.44f) / 8)) * 2;
            }
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
        return new Color(temp.x, temp.y, temp.z);
    }


    // Update is called once per frame
    void Update() {

    }
}
