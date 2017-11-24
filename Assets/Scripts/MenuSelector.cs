using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour {
	GameObject buttonLookingAt;
	GameObject lastButtonLookingAt;
	int layerButton = 9;
	Vector3 offset;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GameObject menu = GameObject.Find ("MenuConstellation");

		if (menu != null) {
			Ray rayEyeCast = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
//			Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
			RaycastHit hit;

			bool rayCasted = Physics.Raycast(rayEyeCast, out hit, layerButton);
			if (rayCasted) {
				buttonLookingAt = hit.transform.gameObject;
				Renderer r = buttonLookingAt.GetComponent<Renderer> ();
				r.material.color = Color.gray;

                if (Input.GetButtonDown("Fire1")) {
					string bname = buttonLookingAt.name;
                    if (bname.Equals ("Save")) {
                        // Save new constellation
                        Debug.Log("save");
                    } else if (bname.Equals ("Name")) {
						// User can give a name to the new constellation
                    } else if (bname.Equals("Cancel")) {
						// Cancel
					}
				}

				lastButtonLookingAt = buttonLookingAt;
			} else {
				if (lastButtonLookingAt != null) {
					Renderer r = lastButtonLookingAt.GetComponent<Renderer> ();
					r.material.color = Color.white;
				}
			}
		}
	}

	public void test() {
		Debug.Log ("Button is pressed !");
	}
}
