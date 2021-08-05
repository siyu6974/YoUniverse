using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lockConstellation : MonoBehaviour {
	private ConstellationData lockedConstellation;
	private Vector3 rotationCenter;
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
		rotationCenter = constellationManager.getConstellationCenter (c);
	}

	private IEnumerator startTurning() {
		Debug.DrawLine (Camera.main.transform.position, rotationCenter, Color.red);
		engaged = true;
        StarGenerator.instance.forceNoTransformation = true;

        Vector3 initPosition = CoordinateManager.virtualPos.galactic;
		Vector3 initRotation = Camera.main.transform.rotation.eulerAngles;
        Vector3 rVect = initPosition - rotationCenter;
		float r = Vector3.Magnitude(rVect);
		float polarOffset = Mathf.Atan(rVect.z / rVect.x);
		if (rVect.x < 0)
            polarOffset += Mathf.PI;
		float elevOffset = Mathf.Asin(rVect.y / r);

		float progress = 0;
        CoordinateManager.exit(Camera.main.transform.position);
        while (progress < 1) {
            float t = progress * progress * (3.0f - 2.0f * progress) * 2f * Mathf.PI;  // ease in and out
			float elevation = t + elevOffset;
			float polar = t + polarOffset;
			float a = r * Mathf.Cos(elevation);
			float x = a * Mathf.Cos(polar);
			float y = r * Mathf.Sin(elevation);
			float z = a * Mathf.Sin(polar);
			Vector3 pos = new Vector3(x, y, z);
            CoordinateManager.virtualPos.galactic = rotationCenter + pos;
			// Camera.main.transform.rotation = Quaternion.Euler(initRotation+new Vector3(0, elevation*180/Mathf.PI, polar*180/Mathf.PI));
            constellationManager.drawConstellation(lockedConstellation);
            constellationManager.clearDrawingWithFadeOut(0.01f);
            yield return new WaitForSeconds(0.01f);
			progress += 1f / (60 * 10);  //  10s at 60fps
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
