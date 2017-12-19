using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperDrive : MonoBehaviour {

    public LaserPointer pointer;

    private StarData? lockedStar;

    public bool engaged { get; private set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        lockStar();

        if (lockedStar != null && Input.GetKeyDown(KeyCode.Semicolon)) {
            StartCoroutine(startWarp());
        }
	}


    bool lockStar() {
        if (Input.GetKey(KeyCode.L) && pointer.pointed != null) {
            lockedStar = (StarData)pointer.pointed;
            Debug.Log("lock");
            return true;
        }
        return false;
    }


    IEnumerator startWarp() {
        engaged = true;
        StarData star = (StarData)lockedStar;
        Vector3 distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        Vector3 delta = distanceVec / 100f;
        CoordinateManager.exit(Camera.main.transform.position);
        while (Vector3.Magnitude(distanceVec) > 2 ) {
            CoordinateManager.virtualPos.galactic += delta;
            yield return new WaitForSeconds(0.01f);
            distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        }
        engaged = false;
    }

}
