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
    public Text setTargetInfoText;
    [HideInInspector] public bool targetSet;
    [HideInInspector] public bool targetGet;
    [HideInInspector] public StarData starTarget;

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
            ajustRotation();

            Ray rayEyeCast = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            //          Debug.DrawRay(Camera.main.transform.position, rayEyeCast.direction * 1000f, Color.cyan);
            RaycastHit hit;

            bool rayCasted = Physics.Raycast(rayEyeCast, out hit, 100.0f, layerButton);
            if (rayCasted) {
                buttonLookingAt = hit.transform.gameObject;
                Renderer r = buttonLookingAt.GetComponent<Renderer>();
                r.material.color = new Color(0.98f, 0.5f, 0.45f);
                Debug.Log(hit.transform.gameObject.name);

                //              if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Fire2")) {
                    string bname = buttonLookingAt.name;
                    if (bname.Equals("Help")) {
                        helpBlock.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.3f + Camera.main.transform.up * 2.95f;
                        helpBlock.transform.rotation = Camera.main.transform.rotation;
                        helpBlock.SetActive(true);
                        hideMenu();
                        return;
                    }
                    if (bname.Equals("Set Target")) {
                        setTargetInfo.SetActive(true);
                        hideMenu();
                        return;
                    }
                }
                //                  string bname = buttonLookingAt.name;
                //                  ConstellationCreater cc = GameObject.Find("_ConstellationMgr").GetComponent<ConstellationCreater>();
                //                  if (bname.Equals ("Save")) {
                //                      // Save new constellation
                //                      cc.saveDrawing();
                //                  } else if (bname.Equals ("AddName")) {
                //                      // User can give a name to the new constellation
                //                  } else if (bname.Equals("Discard")) {
                //                      // Cancel
                //                      cc.discardDrawing();
                //                  }
                //                  hideMenu();
                //              }

                lastButtonLookingAt = buttonLookingAt;
            } else {
                if (lastButtonLookingAt != null) {
                    Renderer r = lastButtonLookingAt.GetComponent<Renderer>();
                    r.material.color = Color.white;
                }
                //              if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
                //              if (Input.GetKeyDown(KeyCode.V)) {
                //                  hideMenu();
                //              }
            }
        }

        if (helpBlock.activeSelf) {
            Debug.Log("In helpBlock: ");
            //if (Input.GetButtonDown("Right Controller Trackpad (Press)")) {
            if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Fire2")) {
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
                starTarget = (StarData)lspointer.pointed;
                if (starTarget.ProperName != "") {
                    setTargetInfoText.text = "Target: " + starTarget.ProperName + "\nDistance: " + starTarget.distance + "\nClick 'B' to confirm and fly to it" + "\nClick 'C' to discard and return";
                } else
                    setTargetInfoText.text = "Target: HIP " + starTarget.HIP + "\nDistance: " + starTarget.distance + "\nClick 'B' to confirm and fly to it" + "\nClick 'C' to discard and return";
                targetGet = true;
            }
            if (targetGet) {
                if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("Fire2")) {
                    // Fly

                }
                if (Input.GetKeyDown(KeyCode.C)) {
                    // Discard
                    setTargetInfo.SetActive(false);
                    flyingInfo.SetActive(true);
                    return;
                }
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
