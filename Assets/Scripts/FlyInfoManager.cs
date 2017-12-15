using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyInfoManager : MonoBehaviour {
	Text infos;
	string speedText, statusText, positionText;
	string speedTitle = "Speed: ";
	string statusTitle = "Status: ";
	string positionTitle = "Position: \n";

	float speed;
	CharacterStates state;
	LandingPhases phase;

	Vector3 position;

    public FlightController controller;

	// Use this for initialization
	void Start () {
		infos = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
//		infos.text = "test line 1\n" + "test line 2";
		speed = controller.getSpeed();
		state = controller.getState();
		phase = controller.getPhase();

		speedText = speedTitle + speed.ToString() + " Units";
		if (state == CharacterStates.landing) {
			statusText = statusTitle + state.ToString () + " - " + phase.ToString ();
		} else {
			statusText = statusTitle + state.ToString ();
		}
        positionText = positionTitle + getPosition();

		infos.text = speedText + "\n" + statusText + "\n" + positionText;
	}


    string getPosition() {
        Vector3? stellar = CoordinateManager.virtualPos.stellar;
        Vector3 galactic = CoordinateManager.virtualPos.galactic;
        string r = "Galactic: " + galactic.ToString() + "\n";
        if (stellar != null)
            r += "Stellar: " + stellar.ToString() + "\n";
        return r;
    }
}
