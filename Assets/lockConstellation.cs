using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lockConstellation : MonoBehaviour {
	private ConstellationData lockedConstellation;
	private Vector3 turningCenter;
	public bool engaged { get; private set; }

	public ConstellationMgr constellationManager;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setConstellationToLock(ConstellationData c) {
		lockedConstellation = c;
		turningCenter = constellationManager.getConstellationCenter (c);
	}

	private IEnumerator startTurning() {
		Debug.DrawLine (Camera.main.transform.position, turningCenter, Color.red);
		engaged = true;
        StarGenerator.instance.forceNoTransformation = true;

        float r = Vector3.Magnitude(CoordinateManager.virtualPos.galactic - turningCenter);
        Vector3 initPosition = CoordinateManager.virtualPos.galactic;
        Vector3 rotationCenter = turningCenter - new Vector3(Mathf.Cos(0) * r, 0, Mathf.Sin(0) * r);

        float t = 0;
        float delta = 2 * Mathf.PI / 5 * 0.01f ; // Turning 360 deg in 5 sec
        CoordinateManager.exit(Camera.main.transform.position);
        while (t < 2 * Mathf.PI) {
            CoordinateManager.virtualPos.galactic = rotationCenter + new Vector3(Mathf.Cos(t)*r, 0, Mathf.Sin(t) * r);
            constellationManager.drawConstellation(lockedConstellation);
            constellationManager.clearDrawingWithFadeOut(0.01f);
            yield return new WaitForSeconds(0.01f);
            t += delta;
		}

        CoordinateManager.virtualPos.galactic = initPosition;
        constellationManager.drawConstellation(lockedConstellation);

		engaged = false;
        StarGenerator.instance.forceNoTransformation = false;
		constellationManager.clearDrawingWithFadeOut();
	}

	public void StartTurning() {
		if (engaged) return;
		StartCoroutine(startTurning());
	}
}
