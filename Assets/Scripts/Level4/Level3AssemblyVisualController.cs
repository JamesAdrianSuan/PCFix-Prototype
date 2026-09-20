using UnityEngine;
using System.Collections.Generic;

public class Level3AssemblyVisualController : MonoBehaviour
{
    [System.Serializable]
    public class AssemblyVisual
    {
        public string componentName;

        [Tooltip(
            "First object should be the ROOT/PARENT of the assembly. " +
            "Other objects can be children/visual parts."
        )]
        public GameObject[] visualObjects;

        [Header("Optional Manual Transform")]
        [Tooltip(
            "Enable this only when the automatic saved transform " +
            "is incorrect for this component."
        )]
        public bool useManualTransform = false;

        [Tooltip("Manual WORLD position used during assembly.")]
        public Vector3 manualWorldPosition;

        [Tooltip("Manual WORLD rotation used during assembly.")]
        public Vector3 manualWorldRotation;

        [Tooltip("Manual LOCAL scale used during assembly.")]
        public Vector3 manualLocalScale = Vector3.one;
    }

    [System.Serializable]
    public class SavedTransformData
    {
        public string componentName;

        // The actual root object used for assembly.
        public GameObject rootObject;

        public Transform parent;

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public Vector3 worldPosition;
        public Quaternion worldRotation;
    }

    [Header("Level 3 Assembly Visuals")]
    public AssemblyVisual[] assemblyVisuals;

    private readonly List<SavedTransformData> savedTransforms =
        new List<SavedTransformData>();

    private void Awake()
    {
        SaveOriginalTransforms();
    }

    private void Start()
    {
        /*
         * IMPORTANT:
         *
         * Do NOT enable all assembly visuals here.
         *
         * Components that were removed during disassembly
         * must remain hidden until the player successfully
         * places them.
         */
    }

    private void SaveOriginalTransforms()
    {
        savedTransforms.Clear();

        if (assemblyVisuals == null ||
            assemblyVisuals.Length == 0)
        {
            Debug.LogWarning(
                "LEVEL 3: No assembly visuals assigned."
            );

            return;
        }

        foreach (AssemblyVisual visual in assemblyVisuals)
        {
            if (visual == null)
                continue;

            if (string.IsNullOrWhiteSpace(
                visual.componentName))
            {
                continue;
            }

            if (visual.visualObjects == null ||
                visual.visualObjects.Length == 0)
            {
                Debug.LogWarning(
                    "LEVEL 3: No visual objects assigned for: " +
                    visual.componentName
                );

                continue;
            }

            GameObject rootObject =
                visual.visualObjects[0];

            if (rootObject == null)
            {
                Debug.LogWarning(
                    "LEVEL 3: First visual object is NULL for: " +
                    visual.componentName
                );

                continue;
            }

            Transform rootTransform =
                rootObject.transform;

            SavedTransformData saved =
                new SavedTransformData();

            saved.componentName =
                visual.componentName.Trim();

            saved.rootObject =
                rootObject;

            saved.parent =
                rootTransform.parent;

            /*
             * MANUAL TRANSFORM OVERRIDE
             *
             * Only components with useManualTransform enabled
             * will use these values.
             *
             * All other components continue using their normal
             * automatic transform saving.
             */
            if (visual.useManualTransform)
            {
                saved.worldPosition =
                    visual.manualWorldPosition;

                saved.worldRotation =
                    Quaternion.Euler(
                        visual.manualWorldRotation
                    );

                saved.localScale =
                    visual.manualLocalScale;

                /*
                 * These are kept for compatibility with the
                 * existing restore system.
                 */
                saved.localPosition =
                    rootTransform.localPosition;

                saved.localRotation =
                    rootTransform.localRotation;

                Debug.Log(
                    "LEVEL 3 MANUAL ROOT SAVED | " +
                    saved.componentName +
                    " | Root=" +
                    rootObject.name +
                    " | Position=" +
                    saved.worldPosition +
                    " | Rotation=" +
                    saved.worldRotation +
                    " | Scale=" +
                    saved.localScale
                );
            }
            else
            {
                /*
                 * NORMAL BEHAVIOR
                 *
                 * All components that do NOT use the manual
                 * override behave exactly like before.
                 */
                saved.parent =
                    rootTransform.parent;

                saved.localPosition =
                    rootTransform.localPosition;

                saved.localRotation =
                    rootTransform.localRotation;

                saved.localScale =
                    rootTransform.localScale;

                saved.worldPosition =
                    rootTransform.position;

                saved.worldRotation =
                    rootTransform.rotation;

                Debug.Log(
                    "LEVEL 3 SAVED ROOT | " +
                    saved.componentName +
                    " | Root=" +
                    rootObject.name +
                    " | Position=" +
                    saved.worldPosition +
                    " | Rotation=" +
                    saved.worldRotation +
                    " | Scale=" +
                    saved.localScale
                );
            }

            savedTransforms.Add(saved);
        }

        Debug.Log(
            "LEVEL 3: Root assembly transforms saved."
        );
    }

    public bool TryGetOriginalTransform(
        string componentName,
        out Vector3 position,
        out Quaternion rotation,
        out Vector3 scale)
    {
        position =
            Vector3.zero;

        rotation =
            Quaternion.identity;

        scale =
            Vector3.one;

        if (string.IsNullOrWhiteSpace(componentName))
            return false;

        string searchName =
            componentName.Trim();

        foreach (SavedTransformData saved
            in savedTransforms)
        {
            if (saved == null)
                continue;

            if (string.Equals(
                saved.componentName,
                searchName,
                System.StringComparison.OrdinalIgnoreCase))
            {
                position =
                    saved.worldPosition;

                rotation =
                    saved.worldRotation;

                scale =
                    saved.localScale;

                Debug.Log(
                    "LEVEL 3 TRANSFORM FOUND | " +
                    searchName +
                    " | Root=" +
                    (
                        saved.rootObject != null
                            ? saved.rootObject.name
                            : "NULL"
                    ) +
                    " | Position=" +
                    position +
                    " | Rotation=" +
                    rotation +
                    " | Scale=" +
                    scale
                );

                return true;
            }
        }

        Debug.LogWarning(
            "LEVEL 3: Could not find saved ROOT transform for: " +
            componentName
        );

        return false;
    }

    public void RestoreComponentToOriginal(
        string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            return;

        string searchName =
            componentName.Trim();

        foreach (SavedTransformData saved
            in savedTransforms)
        {
            if (saved == null)
                continue;

            if (!string.Equals(
                saved.componentName,
                searchName,
                System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (saved.rootObject == null)
            {
                Debug.LogWarning(
                    "LEVEL 3: Saved root object is NULL for: " +
                    componentName
                );

                return;
            }

            Transform root =
                saved.rootObject.transform;

            root.SetParent(
                saved.parent,
                false
            );

            root.localPosition =
                saved.localPosition;

            root.localRotation =
                saved.localRotation;

            root.localScale =
                saved.localScale;

            saved.rootObject.SetActive(true);

            Renderer[] renderers =
                saved.rootObject
                    .GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer
                in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }

            Collider[] colliders =
                saved.rootObject
                    .GetComponentsInChildren<Collider>(true);

            foreach (Collider collider
                in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            Debug.Log(
                "LEVEL 3 ROOT RESTORED | " +
                componentName +
                " | Root=" +
                saved.rootObject.name +
                " | Position=" +
                root.position +
                " | Rotation=" +
                root.rotation +
                " | Scale=" +
                root.localScale
            );

            return;
        }

        Debug.LogWarning(
            "LEVEL 3: Assembly visual not found: " +
            componentName
        );
    }

    public void EnableAllAssemblyVisuals()
    {
        Debug.Log(
            "LEVEL 3: Assembly phase started. " +
            "Removed components remain hidden until placed."
        );
    }

    public void DisableAllAssemblyVisuals()
    {
        Debug.Log(
            "LEVEL 3: Assembly visuals disabled."
        );
    }
}
