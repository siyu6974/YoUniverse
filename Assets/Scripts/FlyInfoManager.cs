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

    public FlightController controller;

	// Use this for initialization
	void Start () {
		infos = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
        speed = controller.speed;
        state = controller.state;
        phase = controller.phase;

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
        StarData? star = CoordinateManager.currentStar; 
        string r = "Galactic: " + galactic.ToString() + "\n";
        if (stellar != null) {
            r += "Stellar: " + stellar.ToString() + "\n";
            if (!((StarData)star).ProperName.Equals("")) 
                r += "Around star : " + ((StarData)star).ProperName;
            else 
                r += "Around star : " + ((StarData)star).HIP;
        }
        return r;
    }
}
