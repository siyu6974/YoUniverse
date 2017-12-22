using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperDrive : MonoBehaviour {
    private StarData? lockedStar;
    public GameObject circlePref;
    public GameObject warpEffectPrefab;

    private GameObject marker;

    public bool engaged { get; private set; }
    public float warpSpeed { get; private set; }
    

    public void lockStar(StarData star) {
        lockedStar = star;
        Camera cam = Camera.main;
        Vector3 pos = Vector3.ClampMagnitude(((StarData)lockedStar).drawnPos - cam.transform.position, 30);

        if (marker != null) Destroy(marker);
        marker = Instantiate(circlePref, cam.transform.position + pos, Quaternion.identity);
        marker.transform.LookAt(cam.transform);
        StartCoroutine(fadeOutMarker(marker, 1f, 2f));
        //Debug.Log("lock");
    }


    private IEnumerator startWarp() {
        engaged = true;
        GameObject warpEffect = Instantiate(warpEffectPrefab, transform.position+Camera.main.transform.forward*10f, Quaternion.identity);
        warpEffect.transform.LookAt(Camera.main.transform);
        WarpEffectController wec = warpEffect.GetComponent<WarpEffectController>();
        wec.playEffect(1.1f);

        StarData star = (StarData)lockedStar;
        Vector3 distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        Vector3 delta = distanceVec / 100f;
        warpSpeed = delta.magnitude;
        CoordinateManager.exit(Camera.main.transform.position);
        while (Vector3.Magnitude(distanceVec) > 2) {
            CoordinateManager.virtualPos.galactic += delta;
            yield return new WaitForSeconds(0.01f);
            distanceVec = star.coord - CoordinateManager.virtualPos.galactic;
        }
        engaged = false;
    }


    private IEnumerator fadeOutMarker(GameObject mrk, float delay, float duration) {
        SpriteRenderer sr = mrk.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(delay);
        float timer = 0f;
        if (sr == null) yield break;
        Color startCol = sr.material.color;
        Color endCol = startCol;
        endCol.a = 0;

        while (timer <= duration) {
            // Set the colour based on the normalised time.
            if (sr == null) yield break;
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
