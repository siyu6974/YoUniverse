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

static public class CoordinateManager {
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

    static public void transformPosition(Vector3 realWorldPos) {
        List<Vector3> l = new List<Vector3>();
        for (int i = 0; i < starDataSet.Length; i++) {
            Vector3 starRelativePos = starDataSet[i].coord - virtualPos.galactic;
            //Debug.Log(starDataSet[i].coord);
            //Debug.Log("g");
            //Debug.Log(virtualPos.galactic);
            //Debug.Log("s");
            //Debug.Log(virtualPos.stellar);
            if (Vector3.Magnitude(starRelativePos) < MyConstants.STAR_SYSTEM_BORDER_ENTRY) {
                if (starSysEntryPt == null) {
                    Debug.Log("Entering");
                    // just enter the system
                    Vector3 flyingDir = (virtualPos.galactic - prevVirtualPos.galactic).normalized;
                    starSysEntryPt = new RVCoordinateBridge {
                        v = flyingDir * MyConstants.STAR_SYSTEM_BORDER_EXIT,
                        r = realWorldPos // record entry point as ref
                    };

                    virtualPos.galactic = starDataSet[i].coord; 
                }
                virtualPos.stellar = realWorldPos - starSysEntryPt.r;
                // exit the system if distance > 1AU
                if (Vector3.Magnitude((Vector3)virtualPos.stellar) > MyConstants.STAR_SYSTEM_BORDER_EXIT)
                    break;
                return;
            }
        }

        if (starSysEntryPt != null) {
            Debug.Log("Exiting");
            // just exit the system
            starSysExitPt.r = realWorldPos;
            starSysExitPt.v = (Vector3)virtualPos.stellar;
            starSysEntryPt = null;
            virtualPos.stellar = null;
        }
        virtualPos.galactic = realWorldPos - starSysExitPt.r + starSysExitPt.v;

        prevVirtualPos = virtualPos;
    }

}
