using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyInfoManager : MonoBehaviour {
	Text infos;
	string speedText, statusText, positionText;
	string speedTitle = "Speed: ";
	string statusTitle = "Status: ";
	string positionTitle = "Position: ";

	float speed;
	enum characterStates { flying = 1, inSpace = 2, landing = 3, onOrbit = 4 };
	characterStates state;
	enum landingPhases { notLanding = 0, preparing = 1, turing = 2, getingDown = 3 };
	landingPhases phase;

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
		state = (characterStates)controller.getState ();
		phase = (landingPhases)controller.getPhase ();
		position = controller.getPosition ();

		speedText = speedTitle + speed.ToString() + " Units";
		if (state == characterStates.landing) {
			statusText = statusTitle + state.ToString () + " - " + phase.ToString ();
		} else {
			statusText = statusTitle + state.ToString ();
		}
		positionText = positionTitle + position.ToString ();

		infos.text = speedText + "\n" + statusText + "\n" + positionText;
	}
}
