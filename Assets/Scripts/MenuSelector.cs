using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelector : MonoBehaviour {
	GameObject buttonLookingAt;
	GameObject lastButtonLookingAt;
	int layerButton = 9;
	Vector3 offset;
    public GameObject menuCanvas;
	[HideInInspector] public bool returnFlag;


	// Use this for initialization
	void Start () {
		returnFlag = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (menuCanvas != null) {
			Ray rayEyeCast = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
//			Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
			RaycastHit hit;

			bool rayCasted = Physics.Raycast(rayEyeCast, out hit, layerButton);
			if (rayCasted) {
				buttonLookingAt = hit.transform.gameObject;
				Renderer r = buttonLookingAt.GetComponent<Renderer> ();
                r.material.color = Color.yellow;

                if (Input.GetButtonDown("Right Controller Trackpad (Press)") || Input.GetKeyDown(KeyCode.B)) {
					string bname = buttonLookingAt.name;
                    ConstellationCreater cc = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationCreater>();
                    if (bname.Equals ("Save")) {
                        // Save new constellation
                        cc.saveDrawing();
						returnFlag = true;
                    } else if (bname.Equals ("AddName")) {
						// User can give a name to the new constellation
						returnFlag = true;
                    } else if (bname.Equals("Discard")) {
						// Cancel
						Debug.Log("Cancel");
                        cc.discardDrawing();
						returnFlag = true;
					}
                    hideMenu();
				}

				lastButtonLookingAt = buttonLookingAt;
			} else {
				if (lastButtonLookingAt != null) {
					Renderer r = lastButtonLookingAt.GetComponent<Renderer> ();
					r.material.color = Color.white;
				}
//                if (Input.GetButtonDown("RMenu")) {
//                    hideMenu();
//                }
			}
		}
	}

	public void test() {
		Debug.Log ("Button is pressed !");
	}

    public void showMenu(){
        Camera cam = Camera.main;
        menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
        menuCanvas.transform.rotation = cam.transform.rotation;
        menuCanvas.SetActive(true);
    }


    public void hideMenu() {
		Renderer r = buttonLookingAt.GetComponent<Renderer> ();
		if (r != null) {
			r.material.color = Color.white;
		}
        menuCanvas.SetActive(false);
        enabled = false;
    }
}
