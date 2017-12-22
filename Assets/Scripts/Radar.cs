using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Radar : MonoBehaviour {
    public GameObject arrowPref;
    public GameObject circlePref;
	public float rotateSpeed = 6.0f; // deg/s

    StarData[] starDataSet;

    // not using struct because marker need to be changed later
    class Marker {
        public StarData star;
        public bool isOnSight;
        public GameObject marker;
    }
    List<Marker> markers = new List<Marker>();

    // Use this for initialization
	void Start () {
        starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;
	}
	
	//// Update is called once per frame
	//void Update () {
 //       if (Input.GetMouseButtonDown(0)) {
 //           Debug.Log("scan");
 //           scanEnvironment();
 //       }
 //       if (Input.GetMouseButton(1)) {
 //           showMarker();
 //       }
	//}


    public void scanEnvironment() {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        clearMarker();
        // find stars within 10 light year, but not within 0.1 lr
        IEnumerable<StarData> ns;
        if (CoordinateManager.currentStar != null)
            ns = from s in starDataSet where s.distance < 100 && s.HIP != ((StarData)CoordinateManager.currentStar).HIP select s;
        else
            ns = from s in starDataSet where s.distance < 100 && s.distance > MyConstants.STAR_SYSTEM_BORDER_ENTRY select s;
         
            
        if (ns.Count() == 0) {
            StarData nearestStar = starDataSet.Aggregate((minItem, nextItem) => (minItem.distance < nextItem.distance && minItem.distance > 1) ? minItem : nextItem);
            Marker m = new Marker {star = nearestStar};
            markers.Add(m);
        } else {
            foreach (StarData s in ns) {
                Marker m = new Marker {star = s};
                markers.Add(m);
            }
        }
    }

    public void showMarker() {
        for (int i = 0; i < markers.Count(); i++) {
            Marker m = markers[i];

            Camera cam = Camera.main;
            Vector3 dir = m.star.drawnPos - cam.transform.position - cam.transform.forward * (cam.farClipPlane * 0.9f);

            Vector3 dirProjectionOnScreen = Vector3.ProjectOnPlane(dir, cam.transform.forward);

            Vector3 starScreenPos = cam.WorldToViewportPoint(m.star.drawnPos);
            if (starScreenPos.z > 0 && starScreenPos.x > 0 && starScreenPos.x < 1 && starScreenPos.y > 0 && starScreenPos.y < 1) {
                // draw circle
                if (!m.isOnSight) {
                    // just get in sight
                    if (m.marker != null) 
                        Destroy(m.marker);
                }
                Vector3 pos = Vector3.ClampMagnitude(m.star.drawnPos - cam.transform.position, 30);
                Debug.DrawRay(cam.transform.position, pos * 1000, Color.red);
                if (m.marker == null) {
                    
                    m.marker = Instantiate(circlePref, cam.transform.position + pos, Quaternion.identity);
                    m.marker.transform.LookAt(cam.transform);
                    StartCoroutine(fadeOutMarker(m, 3f, 2f));
                }
                m.isOnSight = true;

            } else {
                // draw arrow
                Vector3 pos = cam.transform.forward * 10 + Camera.main.transform.position;
                if (m.isOnSight) {
                    // just get out of sight
                    if (m.marker != null)
                        Destroy(m.marker);
                }
                if (m.marker == null) {
                    m.marker = Instantiate(arrowPref, pos, Quaternion.identity);
                    StartCoroutine(fadeOutMarker(m, 3f, 2f));
                }
                m.marker.transform.LookAt(dirProjectionOnScreen, cam.transform.forward * -1);
                m.marker.transform.position = pos + m.marker.transform.forward * 5;
                m.isOnSight = false;

            }
        }
    }


    void clearMarker() {
        foreach (Marker mm in markers) {
            Destroy(mm.marker);
        }
        markers.Clear();
    }


    private IEnumerator fadeOutMarker(Marker m, float delay, float duration) {
        SpriteRenderer sr = m.marker.GetComponent<SpriteRenderer>();
        if (m == null) yield break;
        if (sr ==null) sr = m.marker.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

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
        markers.Remove(m);
    }
}
