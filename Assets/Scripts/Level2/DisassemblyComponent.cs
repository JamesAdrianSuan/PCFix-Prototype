using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisassemblyComponent : MonoBehaviour
{
    [Header("Component")]
    public string componentName;

    [Header("Manager")]
    public DisassemblyManager manager;

    [Header("Movement")]
    public float moveDistance = 1.5f;
    public float moveDuration = 0.5f;

    [Header("Fade")]
    public bool fadeOnRemoval = true;
    public float fadeDuration = 0.4f;

    [Header("GPU Custom Movement")]
    public bool useCustomMovement = false;
    public float customRightDistance = 1.5f;
    public float customUpDistance = 1.0f;

    [Header("Additional Objects")]
    public GameObject[] additionalObjects;

    private Collider[] ownColliders;
    private Renderer[] ownRenderers;

    private Material[] fadeMaterials;

    private bool isInteractable = false;
    private bool isBeingRemoved = false;
    private bool isRemoved = false;

    private void Awake()
    {
        CacheComponents();
        SetupChildClickForwarders();
    }

    private void Start()
    {
        if (manager == null)
            manager = FindAnyObjectByType<DisassemblyManager>();
    }

    // =========================================================
    // CACHE
    // =========================================================

    private void CacheComponents()
    {
        /*
         * IMPORTANT:
         *
         * We collect all colliders/renderers underneath this
         * component, but a child collider that belongs to another
         * DisassemblyComponent is NOT claimed by this component.
         *
         * This prevents:
         *
         * Motherboard
         *   └── CPU Cooler Fan
         *
         * from causing the Motherboard to receive the Fan click.
         */

        List<Collider> colliderList = new List<Collider>();
        List<Renderer> rendererList = new List<Renderer>();

        Collider[] foundColliders =
            GetComponentsInChildren<Collider>(true);

        Renderer[] foundRenderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Collider col in foundColliders)
        {
            if (col == null)
                continue;

            DisassemblyComponent owner =
                FindNearestComponentOwner(col.transform);

            if (owner == this)
            {
                colliderList.Add(col);
            }
        }

        foreach (Renderer rend in foundRenderers)
        {
            if (rend == null)
                continue;

            DisassemblyComponent owner =
                FindNearestComponentOwner(rend.transform);

            if (owner == this)
            {
                rendererList.Add(rend);
            }
        }

        ownColliders = colliderList.ToArray();
        ownRenderers = rendererList.ToArray();
    }

    private DisassemblyComponent FindNearestComponentOwner(
        Transform target
    )
    {
        Transform current = target;

        while (current != null)
        {
            DisassemblyComponent component =
                current.GetComponent<DisassemblyComponent>();

            if (component != null)
                return component;

            current = current.parent;
        }

        return null;
    }

    // =========================================================
    // CLICK FORWARDERS
    // =========================================================

    private void SetupChildClickForwarders()
    {
        if (ownColliders == null)
            return;

        foreach (Collider col in ownColliders)
        {
            if (col == null)
                continue;

            ChildComponentClickForwarder forwarder =
                col.GetComponent<ChildComponentClickForwarder>();

            if (forwarder == null)
            {
                forwarder =
                    col.gameObject.AddComponent<ChildComponentClickForwarder>();
            }

            forwarder.SetOwner(this);
        }
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    public void SetInteractable(bool value)
    {
        isInteractable = value;

        if (ownColliders == null)
            return;

        foreach (Collider col in ownColliders)
        {
            if (col == null)
                continue;

            col.enabled = value;
        }
    }

    public bool IsInteractable()
    {
        return isInteractable &&
               !isBeingRemoved &&
               !isRemoved;
    }

    public bool IsVisibleForInteraction()
    {
        return !isRemoved &&
               !isBeingRemoved &&
               gameObject.activeInHierarchy;
    }

    public void HandleClick()
    {
        /*
         * Only active components are allowed to report clicks.
         *
         * This is important because:
         * - removed parts = ignored
         * - inactive parts = ignored
         * - unrelated objects = never reach this method
         */

        if (!IsInteractable())
            return;

        if (manager == null)
            manager = FindAnyObjectByType<DisassemblyManager>();

        if (manager == null)
            return;

        manager.ComponentClicked(this);
    }

    // =========================================================
    // REMOVAL
    // =========================================================

    public void RemoveComponent()
    {
        if (isBeingRemoved || isRemoved)
            return;

        isBeingRemoved = true;

        SetInteractable(false);

        StartCoroutine(RemovalRoutine());
    }

    private IEnumerator RemovalRoutine()
    {
        Vector3 startPosition = transform.position;

        // -----------------------------------------------------
        // GPU CUSTOM MOVEMENT
        // -----------------------------------------------------

        if (useCustomMovement)
        {
            Vector3 rightTarget =
                startPosition +
                transform.right * customRightDistance;

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(elapsed / moveDuration);

                t = Mathf.SmoothStep(0f, 1f, t);

                transform.position =
                    Vector3.Lerp(
                        startPosition,
                        rightTarget,
                        t
                    );

                yield return null;
            }

            Vector3 upTarget =
                rightTarget +
                Vector3.up * customUpDistance;

            elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(elapsed / moveDuration);

                t = Mathf.SmoothStep(0f, 1f, t);

                transform.position =
                    Vector3.Lerp(
                        rightTarget,
                        upTarget,
                        t
                    );

                yield return null;
            }
        }
        else
        {
            // -------------------------------------------------
            // NORMAL MOVEMENT
            // -------------------------------------------------

            Vector3 targetPosition =
                startPosition +
                Vector3.forward * moveDistance;

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(elapsed / moveDuration);

                t = Mathf.SmoothStep(0f, 1f, t);

                transform.position =
                    Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        t
                    );

                yield return null;
            }
        }

        // -----------------------------------------------------
        // FADE
        // -----------------------------------------------------

        if (fadeOnRemoval)
        {
            PrepareMaterialsForFade();

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;

                float t =
                    Mathf.Clamp01(elapsed / fadeDuration);

                SetFade(t);

                yield return null;
            }
        }

        // -----------------------------------------------------
        // DISABLE
        // -----------------------------------------------------

        DisableRenderers();

        DisableAdditionalObjects();

        isRemoved = true;
        isBeingRemoved = false;

        gameObject.SetActive(false);
    }

    // =========================================================
    // FADE
    // =========================================================

    private void PrepareMaterialsForFade()
    {
        if (ownRenderers == null)
            return;

        List<Material> materials =
            new List<Material>();

        foreach (Renderer renderer in ownRenderers)
        {
            if (renderer == null)
                continue;

            Material[] rendererMaterials =
                renderer.materials;

            foreach (Material material in rendererMaterials)
            {
                if (material != null &&
                    !materials.Contains(material))
                {
                    materials.Add(material);
                }
            }
        }

        fadeMaterials = materials.ToArray();

        foreach (Material material in fadeMaterials)
        {
            if (material == null)
                continue;

            if (material.HasProperty("_Surface"))
                material.SetFloat("_Surface", 1f);

            if (material.HasProperty("_Blend"))
                material.SetFloat("_Blend", 0f);

            if (material.HasProperty("_Alpha"))
                material.SetFloat("_Alpha", 1f);

            if (material.HasProperty("_SrcBlend"))
                material.SetFloat(
                    "_SrcBlend",
                    (float)UnityEngine.Rendering.BlendMode.SrcAlpha
                );

            if (material.HasProperty("_DstBlend"))
                material.SetFloat(
                    "_DstBlend",
                    (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
                );

            if (material.HasProperty("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.renderQueue = 3000;
        }
    }

    private void SetFade(float amount)
    {
        float alpha = 1f - amount;

        if (fadeMaterials == null)
            return;

        foreach (Material material in fadeMaterials)
        {
            if (material == null)
                continue;

            if (material.HasProperty("_Color"))
            {
                Color color =
                    material.GetColor("_Color");

                color.a = alpha;

                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                Color color =
                    material.GetColor("_BaseColor");

                color.a = alpha;

                material.SetColor(
                    "_BaseColor",
                    color
                );
            }

            if (material.HasProperty("_Alpha"))
                material.SetFloat("_Alpha", alpha);
        }
    }

    private void DisableRenderers()
    {
        if (ownRenderers == null)
            return;

        foreach (Renderer renderer in ownRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void DisableAdditionalObjects()
    {
        if (additionalObjects == null)
            return;

        foreach (GameObject obj in additionalObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}


// =============================================================
// CHILD CLICK FORWARDER
// =============================================================

public class ChildComponentClickForwarder : MonoBehaviour
{
    private DisassemblyComponent owner;

    public void SetOwner(DisassemblyComponent component)
    {
        owner = component;
    }

    private void OnMouseDown()
    {
        if (owner == null)
            return;

        owner.HandleClick();
    }
}