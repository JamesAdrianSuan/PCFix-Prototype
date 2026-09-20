using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Level 3 Support")]
    public Level3AssemblyVisualController level3VisualController;

    public bool useLevel3SavedTransform = false;


    // =========================================================
    // RUNTIME
    // =========================================================

    private AssemblyManager manager;

    private bool interactable = false;
    private bool isAssembling = false;
    private bool isAssembled = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Vector3[] originalAdditionalPositions;
    private Quaternion[] originalAdditionalRotations;
    private Vector3[] originalAdditionalScales;

    private Collider[] cachedColliders;
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
    }


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(AssemblyManager assemblyManager)
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
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

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

            Transform t = additionalObjects[i].transform;

            originalAdditionalPositions[i] = t.position;
            originalAdditionalRotations[i] = t.rotation;
            originalAdditionalScales[i] = t.localScale;
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

        foreach (Collider col in cachedAdditionalColliders)
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
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        HandleClick();
    }


    public void HandleClick()
    {
        if (LevelPauseController.IsPaused)
            return;

        if (!interactable ||
            isAssembling ||
            isAssembled)
        {
            return;
        }

        if (manager == null)
        {
            Debug.LogError(
                "AssemblyComponent has NO AssemblyManager: " +
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
        // FINAL TRANSFORM
        // -----------------------------------------------------

        Vector3 finalPosition = originalPosition;
        Quaternion finalRotation = originalRotation;
        Vector3 finalScale = originalScale;


        // -----------------------------------------------------
        // LEVEL 3 SAVED TRANSFORM
        // -----------------------------------------------------

        bool usingLevel3Transform =
            useLevel3SavedTransform &&
            level3VisualController != null;

        if (usingLevel3Transform)
        {
            bool found =
                level3VisualController.TryGetOriginalTransform(
                    componentName,
                    out Vector3 savedPosition,
                    out Quaternion savedRotation,
                    out Vector3 savedScale
                );

            if (found)
            {
                finalPosition = savedPosition;
                finalRotation = savedRotation;
                finalScale = savedScale;

                Debug.Log(
                    "LEVEL 3 ASSEMBLY TARGET | " +
                    componentName +
                    " | Position=" +
                    finalPosition +
                    " | Rotation=" +
                    finalRotation +
                    " | Scale=" +
                    finalScale
                );
            }
            else
            {
                Debug.LogWarning(
                    "LEVEL 3 TRANSFORM NOT FOUND | " +
                    componentName +
                    " | Using AssemblyComponent original transform."
                );
            }
        }


        // -----------------------------------------------------
        // START POSITION
        // -----------------------------------------------------

        Vector3 startPosition;

        Camera cam = Camera.main;


        if (usingLevel3Transform)
        {
            // IMPORTANT:
            // Level 3 starts from the CURRENT detached position.
            // It will smoothly animate toward the saved original
            // assembly position.

            startPosition = transform.position;

            Debug.Log(
                "LEVEL 3 ASSEMBLY START | " +
                componentName +
                " | CurrentPosition=" +
                startPosition +
                " | FinalPosition=" +
                finalPosition
            );
        }
        else
        {
            // -------------------------------------------------
            // LEVEL 2 / NORMAL BEHAVIOR
            // -------------------------------------------------

            Vector3 startOffset;

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

            startPosition =
                finalPosition +
                startOffset;
        }


        // -----------------------------------------------------
        // ADDITIONAL OBJECTS
        // -----------------------------------------------------

        int objectCount =
            additionalObjects != null
                ? additionalObjects.Length
                : 0;

        Vector3[] startAdditionalPositions =
            new Vector3[objectCount];

        bool additionalObjectsAreChildren =
            usingLevel3Transform &&
            AreAdditionalObjectsChildren();


        if (!additionalObjectsAreChildren)
        {
            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                if (usingLevel3Transform)
                {
                    startAdditionalPositions[i] =
                        additionalObjects[i].transform.position;
                }
                else
                {
                    Vector3 startOffset;

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

                    startAdditionalPositions[i] =
                        originalAdditionalPositions[i] +
                        startOffset;
                }
            }
        }


        // -----------------------------------------------------
        // SET INITIAL STATE
        // -----------------------------------------------------

        transform.position =
            startPosition;


        // -----------------------------------------------------
        // ROTATION
        // -----------------------------------------------------

        if (!usingLevel3Transform &&
            snapRotation)
        {
            transform.rotation =
                finalRotation;
        }


        // -----------------------------------------------------
        // ADDITIONAL INITIAL STATE
        // -----------------------------------------------------

        if (!additionalObjectsAreChildren)
        {
            for (int i = 0; i < objectCount; i++)
            {
                if (additionalObjects[i] == null)
                    continue;

                additionalObjects[i]
                    .transform.position =
                    startAdditionalPositions[i];

                if (!usingLevel3Transform &&
                    snapRotation)
                {
                    additionalObjects[i]
                        .transform.rotation =
                        originalAdditionalRotations[i];
                }
            }
        }


        // -----------------------------------------------------
        // ANIMATION
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


            // -------------------------------------------------
            // MAIN COMPONENT POSITION
            // -------------------------------------------------

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    finalPosition,
                    t
                );


            // -------------------------------------------------
            // MAIN COMPONENT ROTATION
            // -------------------------------------------------

            if (snapRotation)
            {
                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        finalRotation,
                        t
                    );
            }


            // -------------------------------------------------
            // ADDITIONAL OBJECTS
            // -------------------------------------------------

            if (!additionalObjectsAreChildren)
            {
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


        // -----------------------------------------------------
        // ADDITIONAL FINAL SNAP
        // -----------------------------------------------------

        if (!additionalObjectsAreChildren)
        {
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
        }


        // -----------------------------------------------------
        // FINAL SCALE
        // -----------------------------------------------------

        transform.localScale =
            finalScale;


        // -----------------------------------------------------
        // LEVEL 3 PSU CHILD TRANSFORMS
        // -----------------------------------------------------

        RestoreLevel3PowerSupplyChildTransforms();


        // -----------------------------------------------------
        // FINAL DIAGNOSTIC
        // -----------------------------------------------------

        if (usingLevel3Transform)
        {
            Debug.Log(
                "===== FINAL LEVEL 3 TRANSFORM AFTER ASSEMBLY ====="
            );

            Debug.Log(
                "COMPONENT: " +
                componentName
            );

            Debug.Log(
                "ROOT NAME: " +
                gameObject.name
            );

            Debug.Log(
                "FINAL POSITION: " +
                transform.position
            );

            Debug.Log(
                "FINAL ROTATION: " +
                transform.rotation.eulerAngles
            );

            Debug.Log(
                "FINAL LOCAL SCALE: " +
                transform.localScale
            );

            Debug.Log(
                "FINAL ACTIVE SELF: " +
                gameObject.activeSelf
            );

            Debug.Log(
                "FINAL ACTIVE HIERARCHY: " +
                gameObject.activeInHierarchy
            );
        }


        // -----------------------------------------------------
        // COMPLETE
        // -----------------------------------------------------

        isAssembled = true;

        isAssembling = false;

        Debug.Log(
            "COMPONENT ASSEMBLED: " +
            componentName
        );
    }


    // =========================================================
    // LEVEL 3 PSU CHILD TRANSFORMS
    // =========================================================

    private void RestoreLevel3PowerSupplyChildTransforms()
    {
        if (!useLevel3SavedTransform)
            return;

        if (!componentName.Trim().Equals(
            "Power Supply",
            System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }


        Transform object4 =
            transform.Find("Object_4");

        if (object4 != null)
        {
            object4.localPosition =
                Vector3.zero;

            object4.localRotation =
                Quaternion.identity;

            object4.localScale =
                Vector3.one;
        }


        Transform object5 =
            transform.Find("Object_5");

        if (object5 != null)
        {
            object5.localPosition =
                Vector3.zero;

            object5.localRotation =
                Quaternion.identity;

            object5.localScale =
                Vector3.one;
        }


        Transform object6 =
            transform.Find("Object_6");

        if (object6 != null)
        {
            object6.localPosition =
                new Vector3(
                    -0.151f,
                    0.02f,
                    0f
                );

            object6.localRotation =
                Quaternion.identity;

            object6.localScale =
                Vector3.one;
        }


        Transform object7 =
            transform.Find("Object_7");

        if (object7 != null)
        {
            object7.localPosition =
                Vector3.zero;

            object7.localRotation =
                Quaternion.identity;

            object7.localScale =
                Vector3.one;
        }


        Transform object8 =
            transform.Find("Object_8");

        if (object8 != null)
        {
            object8.localPosition =
                Vector3.zero;

            object8.localRotation =
                Quaternion.identity;

            object8.localScale =
                Vector3.one;
        }


        Transform object9 =
            transform.Find("Object_9");

        if (object9 != null)
        {
            object9.localPosition =
                Vector3.zero;

            object9.localRotation =
                Quaternion.identity;

            object9.localScale =
                Vector3.one;
        }


        Debug.Log(
            "LEVEL 3 PSU CHILD TRANSFORMS RESTORED"
        );
    }


    // =========================================================
    // CHECK LEVEL 3 CHILDREN
    // =========================================================

    private bool AreAdditionalObjectsChildren()
    {
        if (additionalObjects == null ||
            additionalObjects.Length == 0)
        {
            return false;
        }

        foreach (GameObject obj in additionalObjects)
        {
            if (obj == null)
                continue;

            if (obj.transform.parent != transform)
            {
                return false;
            }
        }

        return true;
    }


    // =========================================================
    // CHILD CLICK FORWARDER
    // =========================================================

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
            if (LevelPauseController.IsPaused)
                return;

            if (parentComponent != null)
            {
                parentComponent.HandleClick();
            }
        }
    }
}
