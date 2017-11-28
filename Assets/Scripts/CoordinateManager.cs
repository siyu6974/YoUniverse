using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct OmniPosition {
    public Vector3 galactic; 
    // 1 unit = 0.1 lr = 632.41077 AU ~= 20x distance between the Sun and pluto
    // origin: Earth

    public Vector3? stellar; 
    // 1 unit = 10^-4 AU
    // origin: entry point
}

static public class CoordinateManager {
    static public StarData[] starDataSet;

    static public OmniPosition virtualPos;

    public struct RVCoordinateBridge {
        public Vector3 r;
        public Vector3 v;
    }

    static public Vector3? stellarSysEntryPt; // in the real world
    static private RVCoordinateBridge stellarSysExitPt = new RVCoordinateBridge {
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
            if (Vector3.Magnitude(starRelativePos) < 1) {
                // if < 20x distance sun-pluto, use stellar system == 632 AU
                if (stellarSysEntryPt == null) {
                    Debug.Log("Entering");
                    // just enter the system
                    stellarSysEntryPt = realWorldPos; // record entry point as ref
                    virtualPos.galactic = starDataSet[i].coord; 
                }
                virtualPos.stellar = realWorldPos - stellarSysEntryPt;
                // exit the system if distance > 1AU
                if (Vector3.Magnitude((Vector3)virtualPos.stellar) > MyConstants.STELLAR_SYSTEM_BORDER)
                    break;
                return;
            }
        }

        if (stellarSysEntryPt != null) {
            Debug.Log("Exiting");
            // just exit the system
            stellarSysExitPt.r = realWorldPos;
            stellarSysExitPt.v = virtualPos.galactic + new Vector3(1f, 0.1f, 0.1f);
            stellarSysEntryPt = null;
            virtualPos.stellar = null;
        }
        virtualPos.galactic = realWorldPos - stellarSysExitPt.r + stellarSysExitPt.v;
    }

}
