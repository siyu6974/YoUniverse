using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    THIS SCRIPT IS WRITTEN BY
    THOMAS KOLE
    www.thomaskole.com

    THIS SCRIPT IS RELEASED UNDER A
    Creative Commons Attribution-NonCommercial
    LISENCE, MEANING THAT YOU CAN USE IT AS LONG
    AS YOU ATTRIBUTE ME.

    FOR COMMERCIAL USE, PLEASE CONTACT ME.

    ----

    THIS SCRIPT READS FROM THE HYG DATABASE
    http://astronexus.com/node/34
    A PROCESSED VERSION CAN BE FOUND IN THE
    .unitypackage,
    OR ON MY WEBSITE
    https://thomaskole.wordpress.com/
    FOR IT'S USAGE, PLEASE SEE THE ASTRONEXUS WEBSITE.

    ----

    PLEASE FEEL FREE TO MODIFY ANY PART OF THIS SCRIPT
    AS MUCH AS YOU WANT!

*/

public class AnalyzeStars : MonoBehaviour {

    ParticleSystem particleSystem;
    public int maxParticles = 100;
    public TextAsset starCSV;
    
    void Awake () {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.maxParticles = maxParticles;
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1];
        bursts[0].minCount = (short)maxParticles;
        bursts[0].maxCount = (short)maxParticles;
        bursts[0].time = 0.0f;
        particleSystem.emission.SetBursts(bursts, 1);
    }

    void Start()
    {
        string[] lines = starCSV.text.Split('\n');

        ParticleSystem.Particle[] particleStars = new ParticleSystem.Particle[maxParticles];
        particleSystem.GetParticles(particleStars);

        for (int i = 0; i < maxParticles; i++)
        {
            string[] components = lines[i].Split(',');
            particleStars[i].position = new Vector3(float.Parse(components[1]),
                                                    float.Parse(components[3]),
                                                    float.Parse(components[2])).normalized * Camera.main.farClipPlane * 0.9f;
            
            particleStars[i].remainingLifetime = Mathf.Infinity;
            particleStars[i].startColor = Color.white * (1.0f - ((float.Parse(components[0]) + 1.44f) / 8));
        }

        particleSystem.SetParticles(particleStars, maxParticles);
    }



}
