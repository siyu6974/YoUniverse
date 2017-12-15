using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct OmniPosition {
    public Vector3 galactic; 
    // 1 unit = 0.1 lr = 632.41077 AU ~= 20x distance between the Sun and pluto
    // origin: Sun. X:?, Y:?, Z:?

    public Vector3? stellar; 
    // 1 unit = 10^-4 AU
    // origin: the sun, X->entrypoint,
    // stellar = null ==> is in interstellar space
}

public class CoordinateManager : MonoBehaviour {
    static public StarData[] starDataSet;

    static public OmniPosition virtualPos = new OmniPosition {
        //stellar = new Vector3(10000, 0, 0) // init at Earth
        stellar = new Vector3(10, 0, 0) // init around the Sun
    };
    static public OmniPosition prevVirtualPos = new OmniPosition();

    public class RVCoordinateBridge {
        public Vector3 r;
        public Vector3 v;
    }

    static public RVCoordinateBridge starSysEntryPt = new RVCoordinateBridge {
        v = new Vector3(MyConstants.STAR_SYSTEM_BORDER_EXIT, 0, 0) // const
    };

    static public RVCoordinateBridge starSysExitPt = new RVCoordinateBridge {
        v = Vector3.zero, r = Vector3.zero
    };


    public delegate void SystemChangeAction();
    public static event SystemChangeAction OnSystemChange;

    static public void transformPosition(Vector3 realWorldPos) {
        // in a star systme
        if (virtualPos.stellar != null) {
            // update position in stellar system 
            virtualPos.stellar = realWorldPos - starSysEntryPt.r;

            // exit the system if distance > 1AU
            if (starSysEntryPt != null && Vector3.Magnitude((Vector3)virtualPos.stellar) > MyConstants.STAR_SYSTEM_BORDER_EXIT) {
                Debug.Log("Exiting");
                // just exit the system
                //Debug.Log((Vector3)virtualPos.stellar);
                //Debug.Log((Vector3)prevVirtualPos.stellar);
                Vector3 flyingDir = ((Vector3)virtualPos.stellar - (Vector3)prevVirtualPos.stellar).normalized;

                starSysExitPt.r = realWorldPos;
                starSysExitPt.v = Vector3.zero;
                starSysEntryPt = null;
                virtualPos.stellar = null;

                // push the caracter away from prev position
                virtualPos.galactic += flyingDir * MyConstants.STAR_SYSTEM_BORDER_ENTRY * 1.5f;
                if (OnSystemChange != null)
                    OnSystemChange();
            }

        } else {
            // in interstellar space
            for (int i = 0; i < starDataSet.Length; i++) {
                Vector3 starRelativePos = starDataSet[i].coord - virtualPos.galactic;
                if (Vector3.Magnitude(starRelativePos) < MyConstants.STAR_SYSTEM_BORDER_ENTRY) {
                    if (starSysEntryPt == null) {
                        Debug.Log("Entering");
                        // just enter the system
                        Vector3 flyingDir = Camera.main.transform.forward;
                        starSysEntryPt = new RVCoordinateBridge {
                            v = -flyingDir * MyConstants.STAR_SYSTEM_BORDER_EXIT,
                            r = realWorldPos // record entry point as ref
                        };
                        //Debug.Log(starSysEntryPt.v);
                        virtualPos.stellar = new Vector3(1, 0, 0) * MyConstants.STAR_SYSTEM_BORDER_EXIT * 0.3f;
                        virtualPos.galactic = starDataSet[i].coord;
                        if (OnSystemChange != null)
                            OnSystemChange();
                    }
                    return;
                }
            }
            virtualPos.galactic = realWorldPos - starSysExitPt.r + starSysExitPt.v;
        }

        prevVirtualPos = virtualPos;
    }


    public static CoordinateManager instance;//singleton

    void Awake() {
        //constructor
        if (instance != null)
            Debug.LogError("Cood Mgr has already been instantiated");
        instance = this;
    }
}
