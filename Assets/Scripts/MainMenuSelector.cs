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
    public GameObject setTargetInfo;
	public GameObject drawConstellationInfo;
	public GameObject scanInfo;
	public GameObject flyToTargetInfo;
	public Text setTargetInfoText;
	public Text flyToTargetInfoText;
	public MenuSelector menuSelector;
	public Radar radar;
    [HideInInspector] public bool targetSet;
    [HideInInspector] public bool targetGet;
    [HideInInspector] public StarData starTarget;

    public HyperDrive hyperDrive;

    // Use this for initialization
    void Start() {
        layerButton = 1 << 9;
        targetGet = false;
        targetSet = false;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("RMenu")) {
            Debug.Log("V pressed");
            if (!menuCanvas.activeSelf) {
                showMenu();
                flyingInfo.SetActive(false);
            } else {
                hideMenu();
                flyingInfo.SetActive(true);
            }
            return;
        }

        //      Camera cam = Camera.main;
        //      menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
        //      Vector3 v = cam.transform.rotation.eulerAngles;
        //      v.y = 0;
        //      menuCanvas.transform.rotation = Quaternion.Euler (v);

        if (menuCanvas.activeSelf) {
            //ajustRotation();

            Ray rayEyeCast = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            //          Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
            RaycastHit hit;

            bool rayCasted = Physics.Raycast(rayEyeCast, out hit, 100.0f, layerButton);
            if (rayCasted) {
                buttonLookingAt = hit.transform.gameObject;
                Renderer r = buttonLookingAt.GetComponent<Renderer>();
                r.material.color = new Color(0.98f, 0.5f, 0.45f);
                Debug.Log(hit.transform.gameObject.name);

                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                    string bname = buttonLookingAt.name;
                    if (bname.Equals("Help")) {
                        helpBlock.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f + Camera.main.transform.up * 2.95f;
                        helpBlock.transform.rotation = Camera.main.transform.rotation;
                        helpBlock.SetActive(true);
                        hideMenu();
                        return;
                    }
                    if (bname.Equals("SetTarget")) {
                        setTargetInfo.SetActive(true);
                        hideMenu();
                        return;
                    }
					if (bname.Equals("DrawConstellation")) {
						drawConstellationInfo.SetActive(true);
						hideMenu();
						return;
					}
					if (bname.Equals ("Scan")) {
						scanInfo.SetActive (true);
						hideMenu();
						return;
					}
                }
                lastButtonLookingAt = buttonLookingAt;
            } else {
                if (lastButtonLookingAt != null) {
                    Renderer r = lastButtonLookingAt.GetComponent<Renderer>();
                    r.material.color = Color.white;
                }
            }
        }

        if (helpBlock.activeSelf) {
            Debug.Log("In helpBlock: ");
            //if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
            if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                helpBlock.SetActive(false);
                flyingInfo.SetActive(true);
                return;
            }
        }
        if (setTargetInfo.activeSelf) {
            Debug.Log("In setTarget Mode: ");
            LaserPointer lspointer = GameObject.Find("RightHand").GetComponent<LaserPointer>();
            if (lspointer.pointed != null) {
                Debug.Log("GetStarTarget");
                hyperDrive.lockStar((StarData)lspointer.pointed);
                starTarget = (StarData)lspointer.pointed;
                if (starTarget.ProperName != "") {
                    setTargetInfoText.text = "Target: " + starTarget.ProperName + "\nDistance: " + starTarget.distance + "\nClick 'B' to confirm and fly to it" + "\nClick 'C' to discard and return";
                } else
                    setTargetInfoText.text = "Target: HIP " + starTarget.HIP + "\nDistance: " + starTarget.distance + "\nClick 'B' to confirm and fly to it" + "\nClick 'C' to discard and return";
                targetGet = true;
            }
            if (targetGet) {
                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                    // Fly
					// -> Change to set target fly mode
					flyToTargetInfo.SetActive(true);
					setTargetInfo.SetActive (false);
					return;
                }
                if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)")) {
                    // Discard
                    setTargetInfo.SetActive(false);
                    flyingInfo.SetActive(true);
					showMenu ();
                    return;
                }
            }
        }
		if (flyToTargetInfo.activeSelf) {
			//Debug.Log ("Now fly to target: ");
			//Vector3 v = transform.position - starTarget.drawnPos;
			//float dist = v.magnitude;
			//flyToTargetInfoText.text = "Target: " + starTarget.ProperName + "\nSpeed: " + "\nDistance: " + dist;
            //StartCoroutine(hyperDrive.startWarp());
            hyperDrive.StartWarp();
        }
		if (drawConstellationInfo.activeSelf) {
			Debug.Log ("In drawConstellation Mode: ");
			ConstellationCreater cc = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationCreater>();
			cc.customCreationMode = true;
            if (!menuSelector.enabled && (Input.GetKeyDown (KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)"))) {
				// Active draw constellation menu
				menuSelector.enabled = true;
				menuSelector.showMenu ();
				return;
			}
			if (menuSelector.returnFlag) {
				// Return to main menu
				drawConstellationInfo.SetActive(false);
				menuSelector.returnFlag = false;
				showMenu ();
				return;
			}
		}
		if (scanInfo.activeSelf) {
			Debug.Log ("In scan Mode: ");
			// Call radar functions
			radar.scanEnvironment();
			radar.showMarker();
            if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)")) {
				scanInfo.SetActive (false);
				showMenu ();
				return;
			}
		}
    }

    public void test() {
        Debug.Log("Button is pressed !");
    }

    public void showMenu() {
        Camera cam = Camera.main;
        menuCanvas.transform.position = cam.transform.position + cam.transform.forward * 3f;
        menuCanvas.transform.rotation = cam.transform.rotation;
        menuCanvas.SetActive(true);
    }


    public void hideMenu() {
        menuCanvas.SetActive(false);
        //      enabled = false;
    }

    public void ajustRotation() {
        Camera cam = Camera.main;
        Vector3 v1 = cam.transform.rotation.eulerAngles;
        Vector3 v2 = menuCanvas.transform.rotation.eulerAngles;
        v2.x = v1.x;
        v2.z = v1.z;
        Quaternion q = Quaternion.Euler(v2);
        menuCanvas.transform.rotation = q;
    }
}
