using UnityEngine;
using UnityEngine.VR;

public enum CharacterStates { flying = 1, inSpace = 2, landing = 3, onOrbit = 4, landed, takingOff };
public enum LandingPhases { notLanding = 0, preparing = 1, turing = 2, getingDown = 3 };


public class FlightController : MonoBehaviour {
    const float defaultSpeed = 4f;
    const float landingSpeed = 2f;
    const float maxOrbitHeight = 50f;

    float speed;

    float height;
    float offsetHeight = 3.0f;

    float distanceForLanding = 20f;
    Vector3 moveDirection = Vector3.zero;
    Quaternion targetRotation;

    Quaternion turningStart;
    float turningTimer;

    CharacterStates state;
    LandingPhases phase;


    [HideInInspector]
    public Transform standingPlanet;

    public Camera cam;
    int layerMask;

    // Use this for initialization
    void Start() {
        state = CharacterStates.inSpace;
        phase = LandingPhases.notLanding;
        speed = defaultSpeed;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        CoordinateManager.OnSystemChange += onCoordSystemChange;
    }

    void FixedUpdate() {
        // Debug for checking down direction for character
        Vector3 directionDown = transform.up * (-1);
        Debug.DrawRay(transform.position, directionDown * 1000f, Color.blue);

        changeCharacterState();
    }


    float getHeightToSurface() {
        float distance = -1.0f;
        Ray landingRay = new Ray(transform.position, transform.up * (-1));
        Debug.DrawRay(transform.position, landingRay.direction * 1000f, Color.yellow);

        RaycastHit landingHit;
        bool rayCasted = Physics.Raycast(landingRay, out landingHit, cam.farClipPlane, layerMask);
        if (rayCasted) {
            distance = landingHit.distance;
        }

        return distance;
    }

    bool detectToLand() {
        Vector3 pos, d;

        Ray forwardRay = new Ray(transform.position, Camera.main.transform.forward);

        // Debug for checking forward direction for character
        Debug.DrawRay(transform.position, forwardRay.direction * 1000f, Color.red);

        RaycastHit hit;
        bool rayCasted = Physics.Raycast(forwardRay, out hit, cam.farClipPlane, layerMask);
        if (rayCasted) {
            pos = hit.point;
            d = pos - transform.position;
            if (d.magnitude <= distanceForLanding) {
                standingPlanet = hit.transform;
                return true;
            }
        }

        return false;
    }

    void changeCharacterState() {
        switch (state) {
            case CharacterStates.flying: {
                    //Debug.Log("Flying");
                    if (!isFlying()) {
                        speed = defaultSpeed;
                        state = CharacterStates.inSpace;
                    } else {
                        moveDirection = Camera.main.transform.forward;
                        if (VRModeDetector.isInVR)
                            speed += (InputTracking.GetLocalPosition(VRNode.RightHand).y - InputTracking.GetLocalPosition(VRNode.CenterEye).y);
                        else
                            speed += 0.3f;
                        moveDirection *= speed;
                        transform.position += (moveDirection * Time.deltaTime);

                        bool toLand = detectToLand();
                        if (toLand) {
                            speed = defaultSpeed;
                            state = CharacterStates.landing;
                            phase = LandingPhases.preparing;
                        }
                    }
                }
                break;
            case CharacterStates.inSpace: {
                    //Debug.Log("In Space");
                    if (isFlying()) {
                        state = CharacterStates.flying;
                    }
                }
                break;
            case CharacterStates.landing: {
                    switch (phase) {
                        case LandingPhases.preparing:
                            //Debug.Log("Preparing");
                            // Character already stopped in this frame
                            Vector3 o = standingPlanet.position;
                            Vector3 p = transform.position;
                            Vector3 po = o - p;
                            // Debug for checking po direction
                            Debug.DrawRay(transform.position, po * 1000f, Color.white);

                            Vector3 directionDown = transform.up * (-1);
                            targetRotation = Quaternion.FromToRotation(directionDown, po);

                            phase = LandingPhases.turing;
                            turningStart = transform.rotation;
                            turningTimer = 0;
                            break;
                        case LandingPhases.turing:
                            if (transform.rotation == targetRotation) {
                                //Debug.Log("Finish turning");
                                phase = LandingPhases.getingDown;
                            } else {
                                //Debug.Log("turning");
                                turningTimer += Time.fixedDeltaTime;
                                transform.rotation = Quaternion.Slerp(turningStart, targetRotation, turningTimer);
                            }

                            break;
                        case LandingPhases.getingDown:
                            //Debug.Log("Geting down");
                            height = getHeightToSurface();
                            if (height >= offsetHeight) {
                                Vector3 dir = transform.up * (-1);
                                dir *= landingSpeed;
                                transform.Translate(dir * Time.deltaTime, Space.World);
                            } else {
                                phase = LandingPhases.notLanding;
                                state = CharacterStates.landed;
                                Debug.Log("Finish landing");
                            }

                            break;
                    }
                }
                break;
            case CharacterStates.onOrbit: {
                    //Debug.Log("OnOrbit");
                    if (isFlying()) {
                        state = CharacterStates.takingOff;
                        break;
                    }

                    // Walking on orbit
                    if (orbitEntryPoint == null) {
                        if (VRModeDetector.isInVR) {
                            orbitEntryPoint = InputTracking.GetLocalPosition(VRNode.CenterEye);
                        } else {
                            orbitEntryPoint = GameObject.Find("TestTrackingObj").transform.position;
                        }
                    }
                    if (orbitEntryPoint != null) {
                        Vector3 entryPoint = (Vector3)orbitEntryPoint;
                        Vector3 currentPos;
                        if (VRModeDetector.isInVR) {
                            currentPos = InputTracking.GetLocalPosition(VRNode.CenterEye);
                        } else {
                            currentPos = GameObject.Find("TestTrackingObj").transform.position;
                        }
                        float theta = (currentPos.x - entryPoint.x) * (2 * Mathf.PI / 10f);
                        float phi = (currentPos.z - entryPoint.z) * (2 * Mathf.PI / 10f);

                        Vector3 transformedPos = new Vector3();
                        transformedPos.x = offsetHeight * Mathf.Sin(theta) * Mathf.Cos(phi);
                        transformedPos.z = offsetHeight * Mathf.Sin(theta) * Mathf.Sin(phi);
                        transformedPos.y = -offsetHeight * Mathf.Cos(theta);

                        transformedPos = transform.InverseTransformVector(transformedPos);

                        transform.position = transformedPos + standingPlanet.position;
                    }
                }
                break;
            case CharacterStates.landed: {
                    if (isFlying()) {
                        state = CharacterStates.takingOff;
                        break;
                    }
                }
                break;
            case CharacterStates.takingOff: {
                    if (!isFlying()) {
                        speed = defaultSpeed;
                        RaycastHit hit;
                        Ray r = new Ray(transform.position, standingPlanet.position - transform.position);

                        Physics.Raycast(r, out hit, cam.farClipPlane, layerMask);
                        float h = hit.distance;

                        if (h > maxOrbitHeight)
                            state = CharacterStates.inSpace;
                        else {
                            offsetHeight = Vector3.Magnitude(transform.position - hit.transform.position);
                            orbitEntryPoint = null;

                            state = CharacterStates.onOrbit;
                        }
                        break;
                    } else {
                        moveDirection = transform.position - standingPlanet.position;
                        moveDirection = moveDirection.normalized;
                        Debug.DrawLine(transform.position, moveDirection*1000, Color.cyan);
                        speed += (InputTracking.GetLocalPosition(VRNode.RightHand).y - InputTracking.GetLocalPosition(VRNode.CenterEye).y);

                        moveDirection *= speed;
                        transform.Translate(moveDirection * Time.deltaTime, Space.World);
                    }
                }
                break;
        }
    }

    bool isFlying() {
        return Input.GetKey(KeyCode.O) || Input.GetButton("Left Controller Trigger (Touch)")
            && Input.GetButton("Left Controller Trackpad (Press)") && Input.GetButton("Right Controller Trackpad (Press)");
    }

    // real world tracking position just after landing / beginning of orbiting
    Vector3? orbitEntryPoint;

    public float getSpeed() {
        return speed;
    }

    public CharacterStates getState() {
        return state;
    }

    public LandingPhases getPhase() {
        return phase;
    }

   
    void onCoordSystemChange() {
        speed = defaultSpeed;   
    }

}
