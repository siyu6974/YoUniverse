using UnityEngine;
using System.Collections;

public class WarpSpeed : MonoBehaviour {
    public float WarpDistortion;
    public float accelaration;
    ParticleSystem particles;
    ParticleSystemRenderer rend;
    bool isWarping = true;

    void Awake() {
        particles = GetComponent<ParticleSystem>();
        rend = particles.GetComponent<ParticleSystemRenderer>();
    }


    private void OnEnable() {
        rend.velocityScale = 0.3f;
    }


    void Update() {
        if (isWarping && !atWarpSpeed()) {
            rend.velocityScale += WarpDistortion * (Time.deltaTime * accelaration);
        }

        //if(!isWarping && !atNormalSpeed())
        //{
        //	rend.velocityScale -= WarpDistortion * (Time.deltaTime * Speed);
        //}
    }

    //public void Engage()
    //{
    //	isWarping = true;
    //}

    //public void Disengage()
    //{
    //	isWarping = false;
    //}

    bool atWarpSpeed() {
        return rend.velocityScale < WarpDistortion;
    }

    bool atNormalSpeed() {
        return rend.velocityScale > 0;
    }
}
