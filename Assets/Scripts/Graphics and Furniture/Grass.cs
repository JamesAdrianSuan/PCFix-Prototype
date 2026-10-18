using UnityEngine;
using UnityEngine.Rendering;

public class Grass : MonoBehaviour
{
    [Header("Outdoor & Indoor Environment Settings")]
    public float totalSize = 500f; // Napakalaking sukat para sakupin ang buong paligid at loob
    public Color grassColor = new Color(0.18f, 0.55f, 0.22f); // Vibrant green lawn

    [Header("Sunny Day Lighting Settings")]
    public Color sunColor = new Color(1f, 0.96f, 0.85f); // Warm sun
    public float sunIntensity = 1.6f;

    void Start()
    {
        SetupFullLawnAndEnvironment();
    }

    void SetupFullLawnAndEnvironment()
    {
        // 1. Gumawa ng isang malaking ground plane para sakupin ang buong loob at labas (Red & Orange areas)
        GameObject fullGrassPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        fullGrassPlane.name = "Full_Lawn_Plane";
        fullGrassPlane.transform.position = new Vector3(0f, -0.1f, 0f);
        fullGrassPlane.transform.localScale = new Vector3(totalSize / 10f, 1f, totalSize / 10f);

        Material grassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        grassMat.SetColor("_BaseColor", grassColor);
        grassMat.SetFloat("_Smoothness", 0.1f);
        fullGrassPlane.GetComponent<Renderer>().material = grassMat;

        // 2. Setup ng Araw (Directional Light) at Liwanag sa Paligid
        GameObject sunObj = GameObject.Find("Directional Light");
        Light sunLight;
        if (sunObj != null) sunLight = sunObj.GetComponent<Light>();
        else
        {
            sunObj = new GameObject("Sunny_Directional_Light");
            sunLight = sunObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
        }
        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;
        sunLight.shadows = LightShadows.Soft;
        sunObj.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.35f;

        Debug.Log("Matagumpay na nailagay ang damo sa buong loob at labas ng laboratory!");
    }
}