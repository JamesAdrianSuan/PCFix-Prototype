using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblyComponent : MonoBehaviour
{
    [Header("Component")]
    public string componentName;

    [Header("Additional Objects")]
    [Tooltip("Independent objects that should assemble together with this component.")]
    public GameObject[] additionalObjects;

    [Header("Assembly Animation")]
    public float assemblyDuration = 0.6f;

    [Tooltip("How far the component starts away from its final position.")]
    public float startingDistance = 0.5f;

    [Header("Placement")]
    [Tooltip("If enabled, the component uses the camera/player perspective for the starting position.")]
    public bool useCameraPerspective = true;

    [Tooltip("Starting offset to the RIGHT from the player's/camera's perspective.")]
    public float startRightOffset = 0.25f;

    [Tooltip("Starting offset UP from the player's/camera's perspective.")]
    public float startUpOffset = 0.15f;

    [Header("Snap")]
    [Tooltip("Snap rotation to the original rotation when assembly finishes.")]
    public bool snapRotation = true;

    [Tooltip("Snap position exactly to the saved assembly position.")]
    public bool snapPosition = true;

    // =========================================================
    // RUNTIME
    // =========================================================

    private AssemblyManager manager;

    private bool interactable = false;
    private bool isAssembling = false;
    private bool isAssembled = false;

    // Saved original transform.
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    // Saved additional-object transforms.
    private Vector3[] originalAdditionalPositions;
    private Quaternion[] originalAdditionalRotations;
    private Vector3[] originalAdditionalScales;

    // Cached colliders.
    private Collider[] cachedColliders;

    // Cached renderers.
    private Renderer[] cachedRenderers;

    private readonly List<Collider> cachedAdditionalColliders =
        new List<Collider>();

    private readonly List<Renderer> cachedAdditionalRenderers =
        new List<Renderer>();

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        SaveOriginalTransforms();
        CacheComponents();
        SetupChildClickForwarders();

        // Assembly starts from the position currently
        // saved in the scene.
        //
        // We will later add an optional "starting position"
        // system if you want the parts to begin outside the PC.
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        AssemblyManager assemblyManager)
    {
        manager = assemblyManager;

        if (cachedColliders == null)
        {
            CacheComponents();
        }

        SetupChildClickForwarders();
    }

    // =========================================================
    // SAVE ORIGINAL TRANSFORMS
    // =========================================================

    private void SaveOriginalTransforms()
    {
        originalPosition =
            transform.position;

        originalRotation =
            transform.rotation;

        originalScale =
            transform.localScale;

        int count =
            additionalObjects != null
                ? additionalObjects.Length
                : 0;

        originalAdditionalPositions =
            new Vector3[count];

        originalAdditionalRotations =
            new Quaternion[count];

        originalAdditionalScales =
            new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            Transform t =
                additionalObjects[i].transform;

            originalAdditionalPositions[i] =
                t.position;

            originalAdditionalRotations[i] =
                t.rotation;

            originalAdditionalScales[i] =
                t.localScale;
        }
    }

    // =========================================================
    // CACHE COMPONENTS
    // =========================================================

    private void CacheComponents()
    {
        cachedColliders =
            GetComponentsInChildren<Collider>(true);

        cachedRenderers =
            GetComponentsInChildren<Renderer>(true);

        cachedAdditionalColliders.Clear();
        cachedAdditionalRenderers.Clear();

        if (additionalObjects == null)
            return;

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

            // Collider belongs directly to this object.
            if (col.gameObject == gameObject)
                continue;

            AssemblyChildClickForwarder forwarder =
                col.GetComponent<AssemblyChildClickForwarder>();

            if (forwarder == null)
            {
                forwarder =
                    col.gameObject.AddComponent<
                        AssemblyChildClickForwarder>();
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
        if (!interactable ||
            isAssembling ||
            isAssembled)
        {
            return;
        }

        if (manager == null)
        {
            Debug.LogError(
                "AssemblyComponent has NO " +
                "AssemblyManager: " +
                gameObject.name
            );

            return;
        }

        Debug.Log(
            "ASSEMBLY COMPONENT CLICKED: " +
            gameObject.name +
            " | Component: " +
            componentName
        );

        manager.ComponentClicked(this);
    }

    // =========================================================
    // ASSEMBLE
    // =========================================================

    public void Assemble()
    {
        if (isAssembling ||
            isAssembled)
        {
            return;
        }

        StartCoroutine(AssemblyRoutine());
    }

    // =========================================================
    // ASSEMBLY ROUTINE
    // =========================================================

    private IEnumerator AssemblyRoutine()
    {
        isAssembling = true;

        SetInteractable(false);

        // -----------------------------------------------------
        // FINAL POSITION
        // -----------------------------------------------------

        Vector3 finalPosition =
            originalPosition;

        Quaternion finalRotation =
            originalRotation;

        // -----------------------------------------------------
        // START POSITION
        //
        // The component begins slightly away from its
        // original position.
        // -----------------------------------------------------

        Vector3 startOffset;

        Camera cam =
            Camera.main;

        if (useCameraPerspective &&
            cam != null)
        {
            startOffset =
                cam.transform.right *
                startRightOffset;

            startOffset +=
                cam.transform.up *
                startUpOffset;
        }
        else
        {
            startOffset =
                Vector3.right *
                startRightOffset;

            startOffset +=
                Vector3.up *
                startUpOffset;
        }

        // Add a little depth separation.
        Vector3 direction;

        if (cam != null)
        {
            direction =
                -cam.transform.forward *
                startingDistance;
        }
        else
        {
            direction =
                Vector3.forward *
                startingDistance;
        }

        startOffset += direction;

        Vector3 startPosition =
            finalPosition +
            startOffset;

        // -----------------------------------------------------
        // ADDITIONAL OBJECT START POSITIONS
        // -----------------------------------------------------

        int objectCount =
            additionalObjects != null
                ? additionalObjects.Length
                : 0;

        Vector3[] startAdditionalPositions =
            new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            startAdditionalPositions[i] =
                originalAdditionalPositions[i] +
                startOffset;
        }

        // -----------------------------------------------------
        // SET INITIAL STATE
        // -----------------------------------------------------

        transform.position =
            startPosition;

        if (snapRotation)
        {
            transform.rotation =
                finalRotation;
        }

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            additionalObjects[i]
                .transform.position =
                startAdditionalPositions[i];

            if (snapRotation)
            {
                additionalObjects[i]
                    .transform.rotation =
                    originalAdditionalRotations[i];
            }
        }

        // -----------------------------------------------------
        // ANIMATE
        // -----------------------------------------------------

        float elapsed = 0f;

        while (elapsed < assemblyDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    assemblyDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            // Main component.
            transform.position =
                Vector3.Lerp(
                    startPosition,
                    finalPosition,
                    t
                );

            if (snapRotation)
            {
                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        finalRotation,
                        t
                    );
            }

            // Additional objects.
            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                additionalObjects[i]
                    .transform.position =
                    Vector3.Lerp(
                        startAdditionalPositions[i],
                        originalAdditionalPositions[i],
                        t
                    );

                if (snapRotation)
                {
                    additionalObjects[i]
                        .transform.rotation =
                        Quaternion.Slerp(
                            additionalObjects[i]
                                .transform.rotation,
                            originalAdditionalRotations[i],
                            t
                        );
                }
            }

            yield return null;
        }

        // -----------------------------------------------------
        // FINAL SNAP
        // -----------------------------------------------------

        if (snapPosition)
        {
            transform.position =
                finalPosition;
        }

        if (snapRotation)
        {
            transform.rotation =
                finalRotation;
        }

        for (int i = 0; i < objectCount; i++)
        {
            if (additionalObjects[i] == null)
                continue;

            if (snapPosition)
            {
                additionalObjects[i]
                    .transform.position =
                    originalAdditionalPositions[i];
            }

            if (snapRotation)
            {
                additionalObjects[i]
                    .transform.rotation =
                    originalAdditionalRotations[i];
            }

            additionalObjects[i]
                .transform.localScale =
                originalAdditionalScales[i];
        }

        transform.localScale =
            originalScale;

        isAssembled = true;
        isAssembling = false;

        Debug.Log(
            "COMPONENT ASSEMBLED: " +
            componentName
        );
    }
}


// =============================================================
// CHILD CLICK FORWARDER
// =============================================================

public class AssemblyChildClickForwarder :
    MonoBehaviour
{
    private AssemblyComponent parentComponent;

    public void SetParentComponent(
        AssemblyComponent component)
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
