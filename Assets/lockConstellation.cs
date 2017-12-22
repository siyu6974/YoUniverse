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
		float delta = 360 / (10 / 0.01f); // Turning 360 deg in 10 sec
		float s = 0;
		while (s < 360) {
			Camera.main.transform.RotateAround (turningCenter, turningCenter + Camera.main.transform.up, delta);
			yield return new WaitForSeconds(0.01f);
			s += delta;
		}
		engaged = false;
		constellationManager.clearDrawingWithFadeOut ();
	}

	public void StartTurning() {
		if (engaged) return;
		constellationManager.drawConstellation (lockedConstellation);
		StartCoroutine(startTurning());
	}
}
