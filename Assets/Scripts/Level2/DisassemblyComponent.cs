using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisassemblyComponent : MonoBehaviour
{
    [Header("Component")]
    public string componentName;

    [Header("Additional Objects")]
    [Tooltip("Independent objects that should be removed together with this component.")]
    public GameObject[] additionalObjects;

    [Header("Removal Animation")]
    public float removalDuration = 0.6f;

    [Header("Custom Animation")]
    public bool useCustomAnimation = false;

    [Tooltip("Movement to the RIGHT from the player's/camera's perspective.")]
    public float rightDistance = 0.15f;

    [Tooltip("Movement UP from the player's/camera's perspective.")]
    public float upwardDistance = 0.35f;

    // =========================================================
    // RUNTIME REFERENCES
    // =========================================================

    private DisassemblyManager manager;

    private bool interactable = false;
    private bool isRemoving = false;

    // Cached colliders.
    private Collider[] cachedColliders;

    // Cached renderers.
    private Renderer[] cachedRenderers;

    // Cached additional-object colliders/renderers.
    private readonly List<Collider> cachedAdditionalColliders =
        new List<Collider>();

    private readonly List<Renderer> cachedAdditionalRenderers =
        new List<Renderer>();

    // =========================================================
    // FADE DATA
    // =========================================================

    private class FadeMaterialData
    {
        public Material material;
        public Color originalColor;
        public float originalAlpha;
    }

    private readonly List<FadeMaterialData> fadeMaterials =
        new List<FadeMaterialData>();

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        CacheComponents();
        SetupChildClickForwarders();
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        DisassemblyManager disassemblyManager)
    {
        manager = disassemblyManager;

        // Cache only if needed.
        if (cachedColliders == null)
        {
            CacheComponents();
        }

        SetupChildClickForwarders();
    }

    // =========================================================
    // CACHE COMPONENTS
    // =========================================================

    private void CacheComponents()
    {
        // -----------------------------------------------------
        // MAIN OBJECT
        // -----------------------------------------------------

        cachedColliders =
            GetComponentsInChildren<Collider>(true);

        cachedRenderers =
            GetComponentsInChildren<Renderer>(true);

        // -----------------------------------------------------
        // ADDITIONAL OBJECTS
        // -----------------------------------------------------

        cachedAdditionalColliders.Clear();
        cachedAdditionalRenderers.Clear();

        if (additionalObjects != null)
        {
            foreach (GameObject obj in additionalObjects)
            {
                if (obj == null)
                    continue;

                Collider[] colliders =
                    obj.GetComponentsInChildren<Collider>(true);

                foreach (Collider col in colliders)
                {
                    if (col != null &&
                        !cachedAdditionalColliders.Contains(col))
                    {
                        cachedAdditionalColliders.Add(col);
                    }
                }

                Renderer[] renderers =
                    obj.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer != null &&
                        !cachedAdditionalRenderers.Contains(renderer))
                    {
                        cachedAdditionalRenderers.Add(renderer);
                    }
                }
            }
        }

        CacheFadeMaterials();
    }

    // =========================================================
    // CACHE FADE MATERIALS
    // =========================================================

    private void CacheFadeMaterials()
    {
        fadeMaterials.Clear();

        // Main renderers.
        if (cachedRenderers != null)
        {
            foreach (Renderer renderer in cachedRenderers)
            {
                CacheRendererMaterials(renderer);
            }
        }

        // Additional renderers.
        foreach (Renderer renderer
            in cachedAdditionalRenderers)
        {
            CacheRendererMaterials(renderer);
        }
    }

    private void CacheRendererMaterials(
        Renderer renderer)
    {
        if (renderer == null)
            return;

        // Use renderer.materials because the fade needs
        // its own material instance.
        Material[] materials =
            renderer.materials;

        foreach (Material material in materials)
        {
            if (material == null)
                continue;

            if (!material.HasProperty("_Color"))
                continue;

            // Prevent duplicate material entries.
            bool alreadyCached = false;

            foreach (FadeMaterialData data
                in fadeMaterials)
            {
                if (data.material == material)
                {
                    alreadyCached = true;
                    break;
                }
            }

            if (alreadyCached)
                continue;

            FadeMaterialData fadeData =
                new FadeMaterialData();

            fadeData.material = material;
            fadeData.originalColor =
                material.color;

            fadeData.originalAlpha =
                material.color.a;

            fadeMaterials.Add(fadeData);
        }
    }

    // =========================================================
    // CHILD CLICK FORWARDERS
    // =========================================================

    private void SetupChildClickForwarders()
    {
        if (cachedColliders == null)
            return;

        foreach (Collider col in cachedColliders)
        {
            if (col == null)
                continue;

            // Collider directly on the parent.
            if (col.gameObject == gameObject)
                continue;

            ChildComponentClickForwarder forwarder =
                col.GetComponent<ChildComponentClickForwarder>();

            if (forwarder == null)
            {
                forwarder =
                    col.gameObject.AddComponent<
                        ChildComponentClickForwarder>();
            }

            forwarder.SetParentComponent(this);
        }
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    public void SetInteractable(bool value)
    {
        interactable = value;

        // Main hierarchy colliders.
        if (cachedColliders != null)
        {
            foreach (Collider col in cachedColliders)
            {
                if (col != null)
                {
                    col.enabled = value;
                }
            }
        }

        // Additional object colliders.
        foreach (Collider col
            in cachedAdditionalColliders)
        {
            if (col != null)
            {
                col.enabled = value;
            }
        }
    }

    // =========================================================
    // CLICK
    // =========================================================

    private void OnMouseDown()
    {
        HandleClick();
    }

    public void HandleClick()
    {
        if (!interactable || isRemoving)
            return;

        if (manager == null)
        {
            Debug.LogError(
                "DisassemblyComponent has NO " +
                "DisassemblyManager: " +
                gameObject.name
            );

            return;
        }

        Debug.Log(
            "COMPONENT CLICKED: " +
            gameObject.name +
            " | Component: " +
            componentName
        );

        manager.ComponentClicked(this);
    }

    // =========================================================
    // REMOVE COMPONENT
    // =========================================================

    public void RemoveComponent()
    {
        if (isRemoving)
            return;

        StartCoroutine(RemovalRoutine());
    }

    // =========================================================
    // REMOVAL ROUTINE
    // =========================================================

    private IEnumerator RemovalRoutine()
    {
        isRemoving = true;

        SetInteractable(false);

        // Prepare transparency.
        PrepareMaterialsForFade();

        Vector3 startPosition =
            transform.position;

        int objectCount =
            additionalObjects != null
                ? additionalObjects.Length
                : 0;

        Vector3[] startPositions =
            new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            startPositions[i] =
                additionalObjects[i].transform.position;
        }

        // =====================================================
        // NORMAL ANIMATION
        // =====================================================

        if (!useCustomAnimation)
        {
            float removalDistance = 0.5f;

            Vector3 direction =
                transform.forward;

            Vector3 targetPosition =
                startPosition +
                direction *
                removalDistance;

            Vector3[] targetPositions =
                new Vector3[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                targetPositions[i] =
                    startPositions[i] +
                    direction *
                    removalDistance;
            }

            float elapsed = 0f;

            while (elapsed < removalDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        removalDuration
                    );

                t =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                transform.position =
                    Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        t
                    );

                for (int i = 0; i < objectCount; i++)
                {
                    if (additionalObjects[i] == null)
                        continue;

                    additionalObjects[i]
                        .transform.position =
                        Vector3.Lerp(
                            startPositions[i],
                            targetPositions[i],
                            t
                        );
                }

                SetFade(t);

                yield return null;
            }

            transform.position =
                targetPosition;

            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                additionalObjects[i]
                    .transform.position =
                    targetPositions[i];
            }

            SetFade(1f);

            DisableRenderers();

            Debug.Log(
                "COMPONENT REMOVED: " +
                componentName
            );

            isRemoving = false;

            yield break;
        }

        // =====================================================
        // CUSTOM CAMERA ANIMATION
        //
        // RIGHT → UP → FADE
        // =====================================================

        Camera cam =
            Camera.main;

        Vector3 rightDirection;
        Vector3 upDirection;

        if (cam != null)
        {
            rightDirection =
                cam.transform.right *
                rightDistance;

            upDirection =
                cam.transform.up *
                upwardDistance;
        }
        else
        {
            rightDirection =
                Vector3.right *
                rightDistance;

            upDirection =
                Vector3.up *
                upwardDistance;
        }

        Vector3 finalOffset =
            rightDirection +
            upDirection;

        Vector3 finalPosition =
            startPosition +
            finalOffset;

        Vector3[] finalAdditionalPositions =
            new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            finalAdditionalPositions[i] =
                startPositions[i] +
                finalOffset;
        }

        float customElapsed = 0f;

        while (customElapsed < removalDuration)
        {
            customElapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    customElapsed /
                    removalDuration
                );

            Vector3 currentOffset;

            // -------------------------------------------------
            // RIGHT
            // -------------------------------------------------

            if (t < 0.45f)
            {
                float rightT =
                    Mathf.InverseLerp(
                        0f,
                        0.45f,
                        t
                    );

                rightT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        rightT
                    );

                currentOffset =
                    rightDirection *
                    rightT;
            }

            // -------------------------------------------------
            // UP
            // -------------------------------------------------

            else
            {
                float upT =
                    Mathf.InverseLerp(
                        0.45f,
                        1f,
                        t
                    );

                upT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        upT
                    );

                currentOffset =
                    rightDirection +
                    upDirection *
                    upT;
            }

            transform.position =
                startPosition +
                currentOffset;

            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                additionalObjects[i]
                    .transform.position =
                    startPositions[i] +
                    currentOffset;
            }

            SetFade(t);

            yield return null;
        }

        // =====================================================
        // FINAL STATE
        // =====================================================

        transform.position =
            finalPosition;

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            additionalObjects[i]
                .transform.position =
                finalAdditionalPositions[i];
        }

        SetFade(1f);

        DisableRenderers();

        Debug.Log(
            "COMPONENT REMOVED: " +
            componentName
        );

        isRemoving = false;
    }

    // =========================================================
    // PREPARE MATERIALS
    // =========================================================

    private void PrepareMaterialsForFade()
    {
        foreach (FadeMaterialData data
            in fadeMaterials)
        {
            if (data.material == null)
                continue;

            Material material =
                data.material;

            string shaderName =
                material.shader != null
                    ? material.shader.name
                    : "";

            // -------------------------------------------------
            // STANDARD
            // -------------------------------------------------

            if (shaderName.Contains("Standard"))
            {
                material.SetFloat(
                    "_Mode",
                    2f
                );

                material.SetInt(
                    "_SrcBlend",
                    (int)UnityEngine.Rendering.BlendMode.SrcAlpha
                );

                material.SetInt(
                    "_DstBlend",
                    (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
                );

                material.SetInt(
                    "_ZWrite",
                    0
                );

                material.DisableKeyword(
                    "_ALPHATEST_ON"
                );

                material.EnableKeyword(
                    "_ALPHABLEND_ON"
                );

                material.DisableKeyword(
                    "_ALPHAPREMULTIPLY_ON"
                );

                material.renderQueue = 3000;
            }

            // -------------------------------------------------
            // URP
            // -------------------------------------------------

            if (shaderName.Contains("Universal Render Pipeline") ||
                shaderName.Contains("URP") ||
                shaderName.Contains("Lit"))
            {
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat(
                        "_Surface",
                        1f
                    );
                }

                if (material.HasProperty("_Blend"))
                {
                    material.SetFloat(
                        "_Blend",
                        0f
                    );
                }

                if (material.HasProperty("_AlphaClip"))
                {
                    material.SetFloat(
                        "_AlphaClip",
                        0f
                    );
                }

                material.EnableKeyword(
                    "_SURFACE_TYPE_TRANSPARENT"
                );

                material.renderQueue = 3000;
            }
        }
    }

    // =========================================================
    // FADE
    // =========================================================

    private void SetFade(float amount)
    {
        float alpha =
            1f -
            Mathf.Clamp01(amount);

        foreach (FadeMaterialData data
            in fadeMaterials)
        {
            if (data.material == null)
                continue;

            Color color =
                data.originalColor;

            color.a =
                data.originalAlpha *
                alpha;

            data.material.color =
                color;
        }
    }

    // =========================================================
    // DISABLE RENDERERS
    // =========================================================

    private void DisableRenderers()
    {
        if (cachedRenderers != null)
        {
            foreach (Renderer renderer
                in cachedRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        foreach (Renderer renderer
            in cachedAdditionalRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}


// =============================================================
// CHILD CLICK FORWARDER
// =============================================================

public class ChildComponentClickForwarder : MonoBehaviour
{
    private DisassemblyComponent parentComponent;

    public void SetParentComponent(
        DisassemblyComponent component)
    {
        parentComponent = component;
    }

    private void OnMouseDown()
    {
        if (parentComponent != null)
        {
            parentComponent.HandleClick();
        }
    }
}
