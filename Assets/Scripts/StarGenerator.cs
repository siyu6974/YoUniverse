using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarGenerator : MonoBehaviour {
    // TODO: https://www.youtube.com/watch?v=-E32N-0qwVM&t=279s
    public TextAsset starCSV;

    public ParticleSystem ps;
    private ParticleSystem.Particle[] starParticles;

    public int starsMax = 100;
    public float starSize = 1;
    public float starDistance = 10;

    // Use this for initialization
    void Start() {
        CreateStars();
    }


    private void CreateStars() {
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
            starParticles[i].startColor = Color.white * (1.0f - ((float.Parse(components[6]) + 1.44f) / 8)) * 2;
            starParticles[i].startSize = starSize;

        }
        ps.SetParticles(starParticles, starParticles.Length);
    }


    // Update is called once per frame
    void Update() {

    }
}
