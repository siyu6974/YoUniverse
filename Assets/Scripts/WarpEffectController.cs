using UnityEngine;
using System.Collections;

public class WarpEffectController : MonoBehaviour {
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

    public void playEffect(float duration) {
        if (!particles.isPlaying) {
            var main = particles.main;
            main.duration = duration;
            main.startLifetime = duration;
        }

        particles.Play();
        Destroy(gameObject, duration+1);
    }
}
