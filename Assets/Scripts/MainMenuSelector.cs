using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSelector : MonoBehaviour {
	GameObject buttonLookingAt;
	GameObject lastButtonLookingAt;
	int layerButton = 9;
	Vector3 offset;
	public GameObject menuCanvas;

	// Use this for initialization
	void Start () {
		hideMenu ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.V)) {
			Debug.Log ("V pressed");
			if (!menuCanvas.activeSelf)
				showMenu ();
			else
				hideMenu ();
		}

//		Camera cam = Camera.main;
//		menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
//		Vector3 v = cam.transform.rotation.eulerAngles;
//		v.y = 0;
//		menuCanvas.transform.rotation = Quaternion.Euler (v);

		if (menuCanvas != null) {
			Ray rayEyeCast = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
			//			Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
			RaycastHit hit;

			bool rayCasted = Physics.Raycast(rayEyeCast, out hit, layerButton);
			if (rayCasted) {
				buttonLookingAt = hit.transform.gameObject;
				Renderer r = buttonLookingAt.GetComponent<Renderer> ();
				r.material.color = new Color(0.98f, 0.5f, 0.45f);

//				if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
//					string bname = buttonLookingAt.name;
//					ConstellationCreater cc = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationCreater>();
//					if (bname.Equals ("Save")) {
//						// Save new constellation
//						cc.saveDrawing();
//					} else if (bname.Equals ("AddName")) {
//						// User can give a name to the new constellation
//					} else if (bname.Equals("Discard")) {
//						// Cancel
//						cc.discardDrawing();
//					}
//					hideMenu();
//				}

				lastButtonLookingAt = buttonLookingAt;
			} else {
				if (lastButtonLookingAt != null) {
					Renderer r = lastButtonLookingAt.GetComponent<Renderer> ();
					r.material.color = Color.white;
				}
//				if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
//				if (Input.GetKeyDown(KeyCode.V)) {
//					hideMenu();
//				}
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
		menuCanvas.SetActive(false);
//		enabled = false;
	}
}
