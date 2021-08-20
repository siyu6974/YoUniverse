using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using System.Linq;

public struct StarData {
    // invar
    public int StarID;
    public int HIP;
    public string BayerFlamsteed;
    public string ProperName;
    public float AbsMag;
    public string Spectrum;
    public Color Color;
    public Vector3 coord; // in parsec/326.156 OR 10^-2 lr

    // var
    public float distance;
    public float Mag;
    public Vector3 drawnPos;
}

public class StarGenerator : MonoBehaviour {
    private const float EYE_RESOLUTION = 0.25f;

    [SerializeField]
    private TextAsset starCSV;

    [HideInInspector]
    public StarData[] starDataSet;
    public ParticleSystem ps;
    private ParticleSystem.Particle[] starParticles;

    public GameObject starPrefab;
    private GameObject nearestStar;

    public int starsMax;

    const float starLinearScale = 19.569f * 2f;
    float lnfovFactor;

    public bool forceNoTransformation;

	[HideInInspector]
	public bool ignoreMovement = false;

    public struct StarPositionUpdateJob : IJobParallelFor
    {
        // In
        [ReadOnly]
        public NativeArray<Vector3> starPoses;

        [ReadOnly]
        public NativeArray<float> absMags;

        public Vector3 obPos;

        public Vector3 camPos;

        // Out
        public NativeArray<Vector3> drawnPoses;
        public NativeArray<float> mags;
        public NativeArray<float> distances;
        public NativeArray<float> starSizes;

        public void Execute(int i) {
            Vector3 starRelativePos = starPoses[i] - obPos;
            drawnPoses[i] = camPos + starRelativePos.normalized * 800;

            var distance = starRelativePos.magnitude;
            distances[i] = distance;
            var mag = calculateMagitude(distance, absMags[i]);
            mags[i] = mag;
            var starSize = starLinearScale * adaptLuminanceScaledLn(pointSourceMagToLnLuminance(mag), .6f);
            if (starSize >= 20) {
                starSize = 20;
            }
            starSizes[i] = starSize;
        }
    }

    StarPositionUpdateJob m_job; 

    void Start() {
        load_data();
        float fov = Camera.main.fieldOfView / 180f;
        double powFactor = Math.Pow(60f / Math.Max(0.7f, fov), 0.8f);
        lnfovFactor = (float)Math.Log(1f / 50f * 2025000f * 60f * 60f / (fov * fov) / (EYE_RESOLUTION * EYE_RESOLUTION) / powFactor / 1.4f);
        Debug.Log(lnfovFactor);

        CoordinateManager.starDataSet = starDataSet;
        CoordinateManager.virtualPos.galactic = Camera.main.transform.position;

        var _starPos = new NativeArray<Vector3>(starsMax, Allocator.Persistent);
        var _starDrawnPos = new NativeArray<Vector3>(starsMax, Allocator.Persistent);
        var _distance = new NativeArray<float>(starsMax, Allocator.Persistent);
        var _mag = new NativeArray<float>(starsMax, Allocator.Persistent);
        var _absMags = new NativeArray<float>(starsMax, Allocator.Persistent);
        var _starSize = new NativeArray<float>(starsMax, Allocator.Persistent);

        var starPosList = from star in starDataSet select star.coord;
        _starPos.CopyFrom(starPosList.ToArray<Vector3>());
        var absMagList = from star in starDataSet select star.AbsMag;
        _absMags.CopyFrom(absMagList.ToArray<float>());

        m_job = new StarPositionUpdateJob() {
            starPoses = _starPos,
            absMags = _absMags,
            // out
            mags = _mag,
            distances = _distance,
            drawnPoses = _starDrawnPos,
            starSizes = _starSize
        };
    }


    // Update is called once per frame
    void Update() {
        // HACK: prevPos is changed in transformPosition, so it needs to be copied before
        var prevPos = CoordinateManager.prevVirtualPos;
        if (!forceNoTransformation)
            CoordinateManager.transformPosition(Camera.main.transform.position);

        if (CoordinateManager.virtualPos != prevPos)
            if (!ignoreMovement)
                createStars(CoordinateManager.virtualPos);
    }


    private void createStars(OmniPosition omniPos) {
        if (omniPos.stellar != null) {
            if (nearestStar == null) {
                //Debug.Log(omniPos.stellar);
                //Debug.Log(CoordinateManager.stellarSysEntryPt);
                Debug.Log("Create star");

                Vector3 dir = -CoordinateManager.starSysEntryPt.v.normalized;
                Vector3 starObjPos = Camera.main.transform.position + dir * Camera.main.farClipPlane * 0.88f;
                nearestStar = Instantiate(starPrefab, starObjPos, Quaternion.identity);
            } else {
                // move and scale it
                //float scaleMod = Vector3.Magnitude(nearestStar.transform.position.normalized*100-(Vector3)omniPos.stellar);
                //nearestStar.transform.localScale = Vector3.one * scaleMod;
            }
            //return;
        }
        if (omniPos.stellar == null && nearestStar != null) {
            Debug.Log("Destory");
            // just exit
            Destroy(nearestStar);
            nearestStar = null;
        }
        Vector3 pos = omniPos.galactic;
        Vector3 camPos = Camera.main.transform.position;
        float farClipPlane = Camera.main.farClipPlane * 0.8f;

        m_job.obPos = pos;
        m_job.camPos = camPos;
        JobHandle handle = m_job.Schedule(starsMax, 100);
        // Wait for the job to complete
        handle.Complete();

        for (int i = 0; i < starsMax; i++) {
            float distance = m_job.distances[i];
            starDataSet[i].distance = distance;
            if (distance < MyConstants.STAR_SYSTEM_BORDER_ENTRY) {
                starParticles[i].startSize = 0f;
                continue;
            }
            starDataSet[i].Mag = m_job.mags[i];
            var dp = m_job.drawnPoses[i];
            starDataSet[i].drawnPos = dp;
            starParticles[i].position = dp;
            starParticles[i].startSize = m_job.starSizes[i];
        }

        ps.SetParticles(starParticles, starParticles.Length);
    }


    // BV < -0.4,+2.0 >
    // Returns Color with RGB <0,1>
    // https://stackoverflow.com/questions/21977786/star-b-v-color-index-to-apparent-rgb-color
    private Color getColor(float bV) {
        int colorIndex = (int)((bV + .5f) / (4f / 127f));
        return colorTable[colorIndex]; // from Stellarium
        //float t, r = 0.0f, g = 0.0f, b = 0.0f;
        //if (bv < -0.4) bv = -0.4f; if (bv > 2.0) bv = 2.0f;
        //    if ((bv >= -.4f) && (bv < .0f)) { t = (bv + .4f) / (.0f + .4f); r = 0.61f + (0.11f * t) + (0.1f * t * t); } else if ((bv >= .0f) && (bv < .4f)) { t = (bv - .0f) / (.4f - .0f); r = 0.83f + (0.17f * t); } else if ((bv >= .4f) && (bv < 2.10f)) { t = (bv - .4f) / (2.10f - .4f); r = 1.00f; }
        //    if ((bv >= -.4f) && (bv < .0f)) { t = (bv + .4f) / (.0f + .4f); g = 0.70f + (0.07f * t) + (0.1f * t * t); } else if ((bv >= .0f) && (bv < .4f)) { t = (bv - .0f) / (.4f - .0f); g = 0.87f + (0.11f * t); } else if ((bv >= .4f) && (bv < 1.60f)) { t = (bv - .4f) / (1.60f - .4f); g = 0.98f - (0.16f * t); } else if ((bv >= 1.60f) && (bv < 2.00f)) { t = (bv - 1.60f) / (2.00f - 1.60f); g = 0.82f - (0.5f * t * t); }
        //    if ((bv >= -.4f) && (bv < .4f)) { t = (bv + .4f) / (.4f + .4f); b = 1.00f; } else if ((bv >= .4f) && (bv < 1.5f)) { t = (bv - .4f) / (1.5f - .4f); b = 1.00f - (0.47f * t) + (0.1f * t * t); } else if ((bv >= 1.50f) && (bv < 1.94)) { t = (bv - 1.50f) / (1.94f - 1.50f); b = 0.63f - (0.6f * t * t); }
        //Vector3 temp = new Vector3(r, g, b).normalized;
        //return new Color(temp.x, temp.y, temp.z, 2555555f);
    }


    void load_data() {
        string[] lines = starCSV.text.Split('\n');
        // StarID,HIP,BayerFlamsteed,ProperName,AbsMag,Spectrum,ColorIndex,X,Y,Z 
        // XYZ in ly

        starDataSet = new StarData[starsMax];
        starParticles = new ParticleSystem.Particle[starsMax];

        for (int i = 0; i < starsMax; i++) {
            string[] components = lines[i+1].Split(',');
            starDataSet[i].StarID = int.Parse(components[0]);
            starDataSet[i].HIP = int.Parse(components[1]);
            starDataSet[i].BayerFlamsteed = components[2];
            starDataSet[i].ProperName = components[3];
            starDataSet[i].AbsMag = float.Parse(components[4]);
            starDataSet[i].Spectrum = components[5];
            try {
                starDataSet[i].Color = getColor(float.Parse(components[6]));
            } catch {
                starDataSet[i].Color = Color.white;
            }
            starDataSet[i].coord.x = float.Parse(components[7])*100;
            starDataSet[i].coord.z = float.Parse(components[8])*100;
            starDataSet[i].coord.y = float.Parse(components[9])*100;
            starParticles[i].startColor = starDataSet[i].Color * 1f;
        }
        starCSV = null;
    }


    static float calculateMagitude(float distance, float M) {
        return (float)(M - 5 * Math.Log10(3260f / distance));
    }

    // Compute the ln of the luminance for a point source with the given mag for the current FOV
    static float pointSourceMagToLnLuminance(float mag){
        // TODO: lnfovFactor is hardcoded
        return -0.92103f*(mag + 12.12331f) + 19.87f;
        // return -0.92103f*(mag + 12.12331f) + lnfovFactor;
    }

    static float adaptLuminanceScaledLn(float lnWorldLuminance, float pFact = 0.5f) {
        const float lnPix0p0001 = -8.0656104861f;
        return (float)Math.Exp(((lnWorldLuminance+lnPix0p0001)*1)*pFact);
    }


    private Color[] colorTable = new Color[128] {
        new Color(0.602745f,0.713725f,1.000000f),
        new Color(0.604902f,0.715294f,1.000000f),
        new Color(0.607059f,0.716863f,1.000000f),
        new Color(0.609215f,0.718431f,1.000000f),
        new Color(0.611372f,0.720000f,1.000000f),
        new Color(0.613529f,0.721569f,1.000000f),
        new Color(0.635490f,0.737255f,1.000000f),
        new Color(0.651059f,0.749673f,1.000000f),
        new Color(0.666627f,0.762092f,1.000000f),
        new Color(0.682196f,0.774510f,1.000000f),
        new Color(0.697764f,0.786929f,1.000000f),
        new Color(0.713333f,0.799347f,1.000000f),
        new Color(0.730306f,0.811242f,1.000000f),
        new Color(0.747278f,0.823138f,1.000000f),
        new Color(0.764251f,0.835033f,1.000000f),
        new Color(0.781223f,0.846929f,1.000000f),
        new Color(0.798196f,0.858824f,1.000000f),
        new Color(0.812282f,0.868236f,1.000000f),
        new Color(0.826368f,0.877647f,1.000000f),
        new Color(0.840455f,0.887059f,1.000000f),
        new Color(0.854541f,0.896470f,1.000000f),
        new Color(0.868627f,0.905882f,1.000000f),
        new Color(0.884627f,0.916862f,1.000000f),
        new Color(0.900627f,0.927843f,1.000000f),
        new Color(0.916627f,0.938823f,1.000000f),
        new Color(0.932627f,0.949804f,1.000000f),
        new Color(0.948627f,0.960784f,1.000000f),
        new Color(0.964444f,0.972549f,1.000000f),
        new Color(0.980261f,0.984313f,1.000000f),
        new Color(0.996078f,0.996078f,1.000000f),
        new Color(1.000000f,1.000000f,1.000000f),
        new Color(1.000000f,0.999643f,0.999287f),
        new Color(1.000000f,0.999287f,0.998574f),
        new Color(1.000000f,0.998930f,0.997861f),
        new Color(1.000000f,0.998574f,0.997148f),
        new Color(1.000000f,0.998217f,0.996435f),
        new Color(1.000000f,0.997861f,0.995722f),
        new Color(1.000000f,0.997504f,0.995009f),
        new Color(1.000000f,0.997148f,0.994296f),
        new Color(1.000000f,0.996791f,0.993583f),
        new Color(1.000000f,0.996435f,0.992870f),
        new Color(1.000000f,0.996078f,0.992157f),
        new Color(1.000000f,0.991140f,0.981554f),
        new Color(1.000000f,0.986201f,0.970951f),
        new Color(1.000000f,0.981263f,0.960349f),
        new Color(1.000000f,0.976325f,0.949746f),
        new Color(1.000000f,0.971387f,0.939143f),
        new Color(1.000000f,0.966448f,0.928540f),
        new Color(1.000000f,0.961510f,0.917938f),
        new Color(1.000000f,0.956572f,0.907335f),
        new Color(1.000000f,0.951634f,0.896732f),
        new Color(1.000000f,0.946695f,0.886129f),
        new Color(1.000000f,0.941757f,0.875526f),
        new Color(1.000000f,0.936819f,0.864924f),
        new Color(1.000000f,0.931881f,0.854321f),
        new Color(1.000000f,0.926942f,0.843718f),
        new Color(1.000000f,0.922004f,0.833115f),
        new Color(1.000000f,0.917066f,0.822513f),
        new Color(1.000000f,0.912128f,0.811910f),
        new Color(1.000000f,0.907189f,0.801307f),
        new Color(1.000000f,0.902251f,0.790704f),
        new Color(1.000000f,0.897313f,0.780101f),
        new Color(1.000000f,0.892375f,0.769499f),
        new Color(1.000000f,0.887436f,0.758896f),
        new Color(1.000000f,0.882498f,0.748293f),
        new Color(1.000000f,0.877560f,0.737690f),
        new Color(1.000000f,0.872622f,0.727088f),
        new Color(1.000000f,0.867683f,0.716485f),
        new Color(1.000000f,0.862745f,0.705882f),
        new Color(1.000000f,0.858617f,0.695975f),
        new Color(1.000000f,0.854490f,0.686068f),
        new Color(1.000000f,0.850362f,0.676161f),
        new Color(1.000000f,0.846234f,0.666254f),
        new Color(1.000000f,0.842107f,0.656346f),
        new Color(1.000000f,0.837979f,0.646439f),
        new Color(1.000000f,0.833851f,0.636532f),
        new Color(1.000000f,0.829724f,0.626625f),
        new Color(1.000000f,0.825596f,0.616718f),
        new Color(1.000000f,0.821468f,0.606811f),
        new Color(1.000000f,0.817340f,0.596904f),
        new Color(1.000000f,0.813213f,0.586997f),
        new Color(1.000000f,0.809085f,0.577090f),
        new Color(1.000000f,0.804957f,0.567183f),
        new Color(1.000000f,0.800830f,0.557275f),
        new Color(1.000000f,0.796702f,0.547368f),
        new Color(1.000000f,0.792574f,0.537461f),
        new Color(1.000000f,0.788447f,0.527554f),
        new Color(1.000000f,0.784319f,0.517647f),
        new Color(1.000000f,0.784025f,0.520882f),
        new Color(1.000000f,0.783731f,0.524118f),
        new Color(1.000000f,0.783436f,0.527353f),
        new Color(1.000000f,0.783142f,0.530588f),
        new Color(1.000000f,0.782848f,0.533824f),
        new Color(1.000000f,0.782554f,0.537059f),
        new Color(1.000000f,0.782259f,0.540294f),
        new Color(1.000000f,0.781965f,0.543529f),
        new Color(1.000000f,0.781671f,0.546765f),
        new Color(1.000000f,0.781377f,0.550000f),
        new Color(1.000000f,0.781082f,0.553235f),
        new Color(1.000000f,0.780788f,0.556471f),
        new Color(1.000000f,0.780494f,0.559706f),
        new Color(1.000000f,0.780200f,0.562941f),
        new Color(1.000000f,0.779905f,0.566177f),
        new Color(1.000000f,0.779611f,0.569412f),
        new Color(1.000000f,0.779317f,0.572647f),
        new Color(1.000000f,0.779023f,0.575882f),
        new Color(1.000000f,0.778728f,0.579118f),
        new Color(1.000000f,0.778434f,0.582353f),
        new Color(1.000000f,0.778140f,0.585588f),
        new Color(1.000000f,0.777846f,0.588824f),
        new Color(1.000000f,0.777551f,0.592059f),
        new Color(1.000000f,0.777257f,0.595294f),
        new Color(1.000000f,0.776963f,0.598530f),
        new Color(1.000000f,0.776669f,0.601765f),
        new Color(1.000000f,0.776374f,0.605000f),
        new Color(1.000000f,0.776080f,0.608235f),
        new Color(1.000000f,0.775786f,0.611471f),
        new Color(1.000000f,0.775492f,0.614706f),
        new Color(1.000000f,0.775197f,0.617941f),
        new Color(1.000000f,0.774903f,0.621177f),
        new Color(1.000000f,0.774609f,0.624412f),
        new Color(1.000000f,0.774315f,0.627647f),
        new Color(1.000000f,0.774020f,0.630883f),
        new Color(1.000000f,0.773726f,0.634118f),
        new Color(1.000000f,0.773432f,0.637353f),
        new Color(1.000000f,0.773138f,0.640588f),
        new Color(1.000000f,0.772843f,0.643824f),
        new Color(1.000000f,0.772549f,0.647059f),
    };

    public static StarGenerator instance;
    void Awake() {
        //constructor
        if (instance != null)
            Debug.LogError("Star Gen has already been instantiated");
        instance = this;
    }

    void OnDestroy() {
        m_job.starPoses.Dispose();
        m_job.absMags.Dispose();
        m_job.mags.Dispose();
        m_job.distances.Dispose();
        m_job.drawnPoses.Dispose();
        m_job.starSizes.Dispose();
    }
}
