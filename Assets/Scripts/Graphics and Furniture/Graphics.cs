using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Graphics : MonoBehaviour
{
    [Header("Sunset Window Light & Flash Settings")]
    public Color sunLightColor = new Color(1f, 0.7f, 0.35f);
    public float sunIntensity = 3.5f;
    public Vector3 sunRotation = new Vector3(25f, -140f, 0f);

    [Header("Realistic & Simple Shadow Settings")]
    [Range(0.1f, 1f)] public float shadowStrength = 0.85f;
    public float shadowDistance = 40f;

    [Header("Environment & Atmosphere")]
    [Range(0f, 2f)] public float ambientIntensity = 1.1f;
    public Color skyboxTint = new Color(1f, 0.7f, 0.5f);

    [Header("Skybox Settings")]
    public Cubemap casualDayCubemap;

    [Header("Post-Processing Settings (Cinematic Flash)")]
    [Range(0f, 5f)] public float bloomIntensity = 1.5f;
    [Range(-100f, 100f)] public float saturation = 25f;
    [Range(-100f, 100f)] public float contrast = 18f;
    [Range(0f, 1f)] public float vignetteIntensity = 0.3f;

    void Start()
    {
        SetupSunsetGraphics();
    }

    void SetupSunsetGraphics()
    {
        // 1. Directional Light at Realistic Simple Shadows Configuration
        Light dirLight = FindAnyObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color = sunLightColor;
            dirLight.intensity = sunIntensity;
            dirLight.transform.rotation = Quaternion.Euler(sunRotation);

            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowStrength = shadowStrength;
            dirLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
            dirLight.shadowBias = 0.05f;
            dirLight.shadowNormalBias = 0.4f;
        }

        // Inayos ang shadow distance gamit ang tamang reference
        QualitySettings.shadowDistance = shadowDistance;

        // 2. Configure Skybox gamit ang Casual Day Cubemap
        if (casualDayCubemap == null)
        {
            casualDayCubemap = Resources.Load<Cubemap>("CasualDay4K");
        }

        if (casualDayCubemap != null)
        {
            Material skyboxMat = new Material(Shader.Find("Skybox/Cubemap"));
            skyboxMat.SetTexture("_Tex", casualDayCubemap);
            RenderSettings.skybox = skyboxMat;
            DynamicGI.UpdateEnvironment();
        }

        // 3. Ambient Lighting (Walang Fog)
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = ambientIntensity;

        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        }

        RenderSettings.fog = false;
        QualitySettings.globalTextureMipmapLimit = 0;

        // 4. Global Volume para sa Post-Processing (Bloom & Flash Effect)
        Volume volume = FindAnyObjectByType<Volume>();
        if (volume == null)
        {
            GameObject volumeObj = new GameObject("Global_PostProcessing_Volume");
            volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        VolumeProfile profile = volume.profile;
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            volume.profile = profile;
        }

        // 5. Tonemapping (ACES)
        if (!profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping = profile.Add<Tonemapping>(true);
        }
        tonemapping.active = true;
        tonemapping.mode.value = TonemappingMode.ACES;
        tonemapping.mode.overrideState = true;

        // 6. Mataas na Bloom para sa Sun Flash sa Bintana
        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }
        bloom.active = true;
        bloom.intensity.value = bloomIntensity;
        bloom.intensity.overrideState = true;
        bloom.threshold.value = 0.75f;
        bloom.threshold.overrideState = true;
        bloom.scatter.value = 0.7f;
        bloom.scatter.overrideState = true;

        // 7. Color Adjustments
        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>(true);
        }
        colorAdjustments.active = true;
        colorAdjustments.saturation.value = saturation;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.contrast.value = contrast;
        colorAdjustments.contrast.overrideState = true;

        // 8. Vignette
        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }
        vignette.active = true;
        vignette.intensity.value = vignetteIntensity;
        vignette.intensity.overrideState = true;
        vignette.smoothness.value = 0.45f;
        vignette.smoothness.overrideState = true;

        Debug.Log("Realistic simple shadows and sunset graphics applied successfully!");
    }
}