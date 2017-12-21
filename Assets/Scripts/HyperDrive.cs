using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperDrive : MonoBehaviour {

    public LaserPointer pointer;

    private StarData? lockedStar;
    public GameObject circlePref;

    public bool engaged { get; private set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}


    public void lockStar(StarData star) {
        lockedStar = star;
        Camera cam = Camera.main;
        Vector3 pos = Vector3.ClampMagnitude(((StarData)lockedStar).drawnPos - cam.transform.position, 30);

        GameObject marker = Instantiate(circlePref, cam.transform.position + pos, Quaternion.identity);
        marker.transform.LookAt(cam.transform);
        StartCoroutine(fadeOutMarker(marker, 1f, 2f));
        Debug.Log("lock");
    }


    private IEnumerator startWarp() {
        engaged = true;
        StarData star = (StarData)lockedStar;
        Vector3 distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        Vector3 delta = distanceVec / 100f;
        CoordinateManager.exit(Camera.main.transform.position);
        while (Vector3.Magnitude(distanceVec) > 2) {
            CoordinateManager.virtualPos.galactic += delta;
            yield return new WaitForSeconds(0.01f);
            distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        }
        engaged = false;
    }


    private IEnumerator fadeOutMarker(GameObject marker, float delay, float duration) {
        SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        yield return new WaitForSeconds(delay);
        float timer = 0f;
        Color startCol = sr.material.color;
        Color endCol = startCol;
        endCol.a = 0;

        while (timer <= duration) {
            // Set the colour based on the normalised time.
            sr.material.color = Color.Lerp(startCol, endCol, timer / duration);

            // Increment the timer by the time between frames and return next frame.
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(sr.gameObject);
    }

    public void StartWarp() {
        if (engaged) return;
        StartCoroutine(startWarp());
    }
}
