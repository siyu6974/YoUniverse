using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSelector : MonoBehaviour {
	GameObject buttonLookingAt;
	GameObject lastButtonLookingAt;

	int layerButton;

	public GameObject menuCanvas;
	public GameObject flyingInfo;
	public GameObject helpBlock;

	// Use this for initialization
	void Start () {
		layerButton = 1 << 9;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.V)) {
			Debug.Log ("V pressed");
			if (!menuCanvas.activeSelf) {
				showMenu ();
				flyingInfo.SetActive (false);
			}
			else {
				hideMenu ();
				flyingInfo.SetActive (true);
			}
			return;
		}

//		Camera cam = Camera.main;
//		menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
//		Vector3 v = cam.transform.rotation.eulerAngles;
//		v.y = 0;
//		menuCanvas.transform.rotation = Quaternion.Euler (v);

		if (menuCanvas.activeSelf) {
			ajustRotation ();

			Ray rayEyeCast = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
			//			Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
			RaycastHit hit;

			bool rayCasted = Physics.Raycast(rayEyeCast, out hit, 100.0f, layerButton);
			if (rayCasted) {
				buttonLookingAt = hit.transform.gameObject;
				Renderer r = buttonLookingAt.GetComponent<Renderer> ();
				r.material.color = new Color (0.98f, 0.5f, 0.45f);
				Debug.Log (hit.transform.gameObject.name);

//				if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
				if (Input.GetKeyDown(KeyCode.B)) {
					string bname = buttonLookingAt.name;
					if (bname.Equals ("Help")) {
						Debug.Log (111);
						helpBlock.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
						helpBlock.transform.rotation = Camera.main.transform.rotation;
						helpBlock.SetActive (true);

						hideMenu ();

					}
				}
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

		if (helpBlock.activeSelf) {
			Debug.Log ("In helpBlock: ");
			Ray rayHelpCast = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
			Debug.DrawRay(Camera.main.transform.position, rayHelpCast.direction * 1000f, Color.cyan);
			RaycastHit helpHit;

			bool rayHelpCasted = Physics.Raycast (rayHelpCast, out helpHit, 1000.0f, layerButton);
			if (rayHelpCasted) {
				GameObject obj = helpHit.transform.gameObject;
				Renderer r = obj.GetComponent<Renderer> ();
				r.material.color = Color.gray;
				Debug.Log (helpHit.transform.gameObject.name);

				//if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
				if (Input.GetKeyDown (KeyCode.B)) {
					string bname = helpHit.transform.gameObject.name;
					if (bname.Equals ("OK")) {
						helpBlock.SetActive (false);
						flyingInfo.SetActive (true);
					}
				}
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

	public void ajustRotation() {
		Camera cam = Camera.main;
		Vector3 v1 = cam.transform.rotation.eulerAngles;
		Vector3 v2 = menuCanvas.transform.rotation.eulerAngles;
		v2.x = v1.x;
		v2.z = v1.z;
		Quaternion q = Quaternion.Euler (v2);
		menuCanvas.transform.rotation = q;
	}
}
