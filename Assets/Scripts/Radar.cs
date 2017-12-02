using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Radar : MonoBehaviour {
    public GameObject arrow;

    StarData[] starDataSet;
    List<StarData> nearStars = new List<StarData>();

	// Use this for initialization
	void Start () {
        starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;
        StartCoroutine(test());
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            scanEnvironment();

            showMarker();
        }
	}


    public void scanEnvironment() {
        if (starDataSet == null) starDataSet = GameObject.Find("_StarGenerator").GetComponent<StarGenerator>().starDataSet;

        // find stars within 10 light year
        //IEnumerable<StarData> ns = from s in starDataSet where s.distance < 100 select s;
        //if (ns.Count() == 0) {
        //    StarData nearestStar = starDataSet.Aggregate((minItem, nextItem) => minItem.distance < nextItem.distance ? minItem : nextItem);
        //    nearStars.Add(nearestStar);
        //} else {
        //    nearStars = ns.ToList();
        //}
        Debug.Log(starDataSet[1].ProperName);
        nearStars.Add(starDataSet[1]);
    }

    public void showMarker() {
        foreach (StarData star in nearStars) {
            Vector3 pos = Camera.main.transform.forward * 10 + Camera.main.transform.position;
            Vector3 dir = star.drawnPos - Camera.main.transform.position - Camera.main.transform.forward * (Camera.main.farClipPlane * 0.9f);

            Debug.Log(star.drawnPos);
            Debug.Log(dir);

            Vector3 reff = new Vector3(0, 0, 90);

            //dir += arrow.transform.rotation.eulerAngles;
            //Quaternion rotation = Quaternion.LookRotation(dir);
            GameObject go = Instantiate(arrow, pos, Quaternion.identity);
            go.transform.LookAt(dir, Camera.main.transform.forward*-1);
        }
    }


    IEnumerator test() {
        yield return new WaitForSeconds(4);
        scanEnvironment();
        showMarker();
    }

}
