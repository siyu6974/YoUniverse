using UnityEngine;
using UnityEngine.VR;

public enum CharacterStates { flying = 1, inSpace = 2, landing = 3, onOrbit = 4, landed, takingOff };
public enum LandingPhases { notLanding = 0, preparing = 1, turing = 2, getingDown = 3 };


public class FlightController : MonoBehaviour {
    const float defaultSpeed = 4f;
    const float landingSpeed = 2f;

    const float maxOrbitHeight = 50f;
    const float distanceForLanding = 20f;

    float offsetHeight = 2f;

    public float speed { get; private set; }
    Vector3 moveDirection = Vector3.zero;

    Quaternion targetRotation;
    Quaternion turningStart;
    float turningTimer;

    public CharacterStates state { get; private set; }
    public LandingPhases phase { get; private set; }

    public Transform standingPlanet { get; private set; }

    private Camera cam;
    private Transform camT;
    int layerMask;


    // Use this for initialization
    void Start() {
        state = CharacterStates.inSpace;
        phase = LandingPhases.notLanding;
        speed = defaultSpeed;
        layerMask = 1 << 8;
        layerMask = ~layerMask;
        cam = Camera.main;
        camT = cam.transform;
        CoordinateManager.OnSystemChange += onCoordSystemChange;
    }

    void FixedUpdate() {
        // Debug for checking down direction for character
        //Vector3 directionDown = camT.up * (-1);
        //Debug.DrawRay(camT.position, directionDown * 1000f, Color.blue);

        changeCharacterState();
    }


    float getHeightToSurface() {
        float distance = -1.0f;
        Ray landingRay = new Ray(camT.position, camT.up * (-1));
        Debug.DrawRay(camT.position, landingRay.direction * 1000f, Color.yellow);

        RaycastHit landingHit;
        bool rayCasted = Physics.Raycast(landingRay, out landingHit, cam.farClipPlane, layerMask);
        if (rayCasted) {
            distance = landingHit.distance;
        }

        return distance;
    }

    bool detectToLand() {
        Vector3 pos, d;

        Ray forwardRay = new Ray(camT.position, camT.forward);

        // Debug for checking forward direction for character
        Debug.DrawRay(camT.position, forwardRay.direction * 1000f, Color.red);

        RaycastHit hit;
        bool rayCasted = Physics.Raycast(forwardRay, out hit, cam.farClipPlane, layerMask);
        if (rayCasted) {
            pos = hit.point;
            d = pos - camT.position;
            if (d.magnitude <= distanceForLanding) {
                standingPlanet = hit.transform;
                return true;
            }
        }

        return false;
    }

    void changeCharacterState() {
        switch (state) {
            case CharacterStates.flying: 
                //Debug.Log("Flying");
                if (!isFlying()) {
                    speed = defaultSpeed;
                    state = CharacterStates.inSpace;
                } else {
                    moveDirection = camT.forward;
                    if (VRModeDetector.isInVR)
                        speed += (UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand).y - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye).y);
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
                
                break;
            case CharacterStates.inSpace: 
                //Debug.Log("In Space");
                if (isFlying()) {
                    state = CharacterStates.flying;
                }
                
                break;
            case CharacterStates.landing: 
                switch (phase) {
                    case LandingPhases.preparing:
                        //Debug.Log("Preparing");
                        // Character already stopped in this frame
                        Vector3 o = standingPlanet.position;
                        Vector3 p = camT.position;
                        Vector3 po = o - p;
                        // Debug for checking po direction
                        Debug.DrawRay(camT.position, po * 1000f, Color.white);

                        Vector3 directionDown = camT.up * (-1);
                        targetRotation = transform.rotation * Quaternion.FromToRotation(directionDown, po);
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
                        float height = getHeightToSurface();
                        if (height >= offsetHeight) {
                            Vector3 dir = camT.up * (-1);
                            dir *= landingSpeed;
                            transform.Translate(dir * Time.deltaTime, Space.World);
                        } else {
                            phase = LandingPhases.notLanding;
                            state = CharacterStates.landed;
                            Debug.Log("Finish landing");
                        }

                        break;
                }
                
                break;
            case CharacterStates.onOrbit: 
                //Debug.Log("OnOrbit");
                if (isFlying()) {
                    state = CharacterStates.takingOff;
                    break;
                }

                // Walking on orbit
                if (orbitEntryPoint == null) {
                    if (VRModeDetector.isInVR) {
                        orbitEntryPoint = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                    } else {
                        orbitEntryPoint = GameObject.Find("TestTrackingObj").transform.position;
                    }
                }
                if (orbitEntryPoint != null) {
                    Vector3 entryPoint = (Vector3)orbitEntryPoint;
                    Vector3 currentPos;
                    if (VRModeDetector.isInVR) {
                        currentPos = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                    } else {
                        currentPos = GameObject.Find("TestTrackingObj").transform.position;
                    }
                    float theta = (currentPos.x - entryPoint.x) * (2 * Mathf.PI / 5f);
                    float phi = (currentPos.z - entryPoint.z) * (2 * Mathf.PI / 5f);

                    Vector3 transformedPos = new Vector3();
                    transformedPos.x = offsetHeight * Mathf.Sin(theta) * Mathf.Cos(phi);
                    transformedPos.z = offsetHeight * Mathf.Sin(theta) * Mathf.Sin(phi);
                    transformedPos.y = -offsetHeight * Mathf.Cos(theta);

                    transformedPos = transform.InverseTransformVector(transformedPos);

                    transform.position = transformedPos + standingPlanet.position;
                }
                
                break;
            case CharacterStates.landed: 
                if (isFlying()) {
                    state = CharacterStates.takingOff;
                    break;
                }
                break;
            case CharacterStates.takingOff: 
                if (!isFlying()) {
                    speed = defaultSpeed;
                    RaycastHit hit;
                    Ray r = new Ray(camT.position, standingPlanet.position - camT.position);

                    Physics.Raycast(r, out hit, cam.farClipPlane, layerMask);
                    float h = hit.distance;

                    if (h > maxOrbitHeight)
                        state = CharacterStates.inSpace;
                    else {
                        offsetHeight = Vector3.Magnitude(camT.position - hit.transform.position);
                        orbitEntryPoint = null;

                        state = CharacterStates.onOrbit;
                    }
                    break;
                } else {
                    moveDirection = camT.position - standingPlanet.position;
                    moveDirection = moveDirection.normalized;
                    Debug.DrawLine(camT.position, moveDirection*1000, Color.cyan);
                    speed += (UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand).y - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye).y);

                    moveDirection *= speed;
                    transform.Translate(moveDirection * Time.deltaTime, Space.World);
                }

                break;
        }
    }

    bool isFlying() {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Left Controller Trigger (Touch)")
            && Input.GetButton("Left Controller Trackpad (Press)") && Input.GetButton("Right Controller Trackpad (Press)");
    }

    // real world tracking position just after landing / beginning of orbiting
    Vector3? orbitEntryPoint;

   
    void onCoordSystemChange() {
        speed = defaultSpeed;   
    }

}
