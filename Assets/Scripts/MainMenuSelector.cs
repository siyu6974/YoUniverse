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
    public GameObject warpDriveInfo;
	public GameObject drawConstellationInfo;
	public GameObject scanInfo;
	public GameObject flyToTargetInfo;
	public Text warpDriveInfoText;
	public Text flyToTargetInfoText;
	public GameObject lockConstellationInfo;
	public GameObject turnAroundInfo;
	public Text setTargetInfoText;
	public Text lockConstellationInfoText;
	public MenuSelector menuSelector;
	public Radar radar;
    [HideInInspector] public bool targetSet;
    [HideInInspector] public bool targetGet;
	[HideInInspector] public bool constellationSet;
	[HideInInspector] public bool constellationGet;
    [HideInInspector] public StarData starTarget;
	[HideInInspector] public ConstellationData constellationTarget;

    public HyperDrive hyperDrive;
    public LaserPointer laserPointer;
	public lockConstellation locker;

    // Use this for initialization
    void Start() {
        layerButton = 1 << 9;
        targetGet = false;
        targetSet = false;
		constellationGet = false;
		constellationSet = false;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.V) || Input.GetButtonDown("RMenu")) {
            // Debug.Log("V pressed");
            if (!menuCanvas.activeSelf) {
                showMenu();
                flyingInfo.SetActive(false);
            } else {
                hideMenu();
                flyingInfo.SetActive(true);
            }
            return;
        }

        if (menuCanvas.activeSelf) {
            // ajustRotation();

            Ray rayEyeCast = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            bool rayCasted = Physics.Raycast(rayEyeCast, out hit, 100.0f, layerButton);
            if (rayCasted) {
                buttonLookingAt = hit.transform.gameObject;
                Renderer r = buttonLookingAt.GetComponent<Renderer>();
                r.material.color = new Color(0.98f, 0.5f, 0.45f);
                // Debug.Log(hit.transform.gameObject.name);

                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                    string bname = buttonLookingAt.name;
                    if (bname.Equals("Help")) {
                        helpBlock.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f + Camera.main.transform.up * 2.95f;
                        helpBlock.transform.rotation = Camera.main.transform.rotation;
                        helpBlock.SetActive(true);
                        hideMenu();
                        return;
                    }
                    if (bname.Equals("WarpDrive")) {
                        warpDriveInfo.SetActive(true);
						warpDriveInfoText.text = "Use laser to choose a star as target:\nPress right controller trigger";
                        hideMenu();
                        return;
                    }
					if (bname.Equals("LockConstellation")) {
						lockConstellationInfo.SetActive(true);
						lockConstellationInfoText.text = "Use laser to choose a constellation to lock:\nPress right controller trigger";
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
            // Debug.Log("In helpBlock: ");
            if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                helpBlock.SetActive(false);
                flyingInfo.SetActive(true);
                return;
            }
        }
        if (warpDriveInfo.activeSelf) {
            // Debug.Log("In warpDrive Mode: ");
            if (laserPointer.pointed != null) {
                //Debug.Log("GetStarTarget");
                hyperDrive.lockStar((StarData)laserPointer.pointed);
                starTarget = (StarData)laserPointer.pointed;
                if (starTarget.ProperName != "") {
                    warpDriveInfoText.text = "Target: " + starTarget.ProperName + "\nDistance: " + starTarget.distance + "ly\nPress right controller trackpad to confirm and fly to it" + "\nPress left controller trackpad to discard and return";
                } else
					warpDriveInfoText.text = "Target: HIP " + starTarget.HIP + "\nDistance: " + starTarget.distance + "ly\nPress right controller trackpad to confirm and fly to it" + "\nPress left controller trackpad to discard and return";
                targetGet = true;
            }
            if (targetGet) {
                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                    // Fly
					flyToTargetInfo.SetActive(true);
					warpDriveInfo.SetActive (false);
					return;
                }
                if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)")) {
                    // Discard
                    warpDriveInfo.SetActive(false);
                    flyingInfo.SetActive(true);
                    return;
                }
            }
        }
		if (flyToTargetInfo.activeSelf) {
			// Debug.Log ("Now fly to target: ");
            hyperDrive.StartWarp();
			flyToTargetInfo.SetActive (false);
			flyingInfo.SetActive (true);
			return;
        }
		if (lockConstellationInfo.activeSelf) {
			// Debug.Log ("In lockConstellation Mode: ");
			LaserPointer lspointer = GameObject.Find("RightHand").GetComponent<LaserPointer>();
			if (lspointer.pointed != null) {
				Debug.Log("GetStarPointed");
				// hyperDrive.lockStar((StarData)lspointer.pointed);
				if (lspointer.selected != null) {
					constellationTarget = (ConstellationData)lspointer.selected;
					lockConstellationInfoText.text = "Constellation: " + constellationTarget.name + "\nPress right controller trackpad to lock" + "\nPress left controller trackpad to discard and return";
					constellationGet = true;
				} 
			}
			if (constellationGet) {
				if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Right Controller Trackpad (Press)")) {
					// Turn around
					locker.setConstellationToLock(constellationTarget);
					turnAroundInfo.SetActive(true);
					lockConstellationInfo.SetActive (false);
					return;
				}
				if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)")) {
					// Discard
					lockConstellationInfo.SetActive(false);
					flyingInfo.SetActive(true);
					return;
				}
			}
		}
		if (turnAroundInfo.activeSelf) {
			locker.StartTurning ();
			turnAroundInfo.SetActive (false);
			flyingInfo.SetActive (true);
			return;
		}
		if (drawConstellationInfo.activeSelf) {
			// Debug.Log ("In drawConstellation Mode: ");
			Text[] texts = drawConstellationInfo.GetComponentsInChildren<Text> ();
			ConstellationCreater cc = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationCreater>();
			cc.customCreationMode = true;
            if (!menuSelector.enabled && (Input.GetKeyDown (KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)"))) {
				// Active draw constellation menu
				menuSelector.enabled = true;
				texts[1].text = "";
				menuSelector.showMenu ();
				return;
			}
			if (menuSelector.returnFlag) {
				// Return
				texts[1].text = "Press and hold right controller trigger and trackpad to draw\nWhen finish, press left controller trackpad";
				drawConstellationInfo.SetActive(false);
				menuSelector.returnFlag = false;
				flyingInfo.SetActive (true);
				return;
			}
		}
		if (scanInfo.activeSelf) {
			// Debug.Log ("In scan Mode: ");
			// Call radar functions
			radar.scanEnvironment();
			radar.showMarker();
            if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Left Controller Trackpad (Press)")) {
				scanInfo.SetActive (false);
				flyingInfo.SetActive (true);
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
		Renderer r = buttonLookingAt.GetComponent<Renderer>();
		if (r != null) {
			r.material.color = Color.white;
		}
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
