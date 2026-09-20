using UnityEngine;

public class AssemblyDropTarget : MonoBehaviour
{
    [Header("Target")]
    public string componentName;

    [Header("Assembly Component")]
    public AssemblyComponent assemblyComponent;

    [Header("Level 3 Optional")]
    public Level3AssemblyVisualController level3VisualController;

    private void OnMouseDown()
    {
        Debug.Log(
            "TARGET HIT: " +
            componentName +
            " | Object: " +
            gameObject.name
        );
    }

    public bool IsCorrectComponent(string draggedName)
    {
        if (string.IsNullOrWhiteSpace(draggedName))
            return false;

        return string.Equals(
            draggedName.Trim(),
            componentName.Trim(),
            System.StringComparison.OrdinalIgnoreCase
        );
    }
public void PlaceComponent()
    {
        if (assemblyComponent == null)
        {
            Debug.LogError(
                "NO AssemblyComponent assigned to target: " +
                gameObject.name
            );

            return;
        }

        Debug.Log(
            "PLACING COMPONENT: " +
            componentName
        );

        /*
         * LEVEL 3
         *
         * Only Level 3 targets use the saved original
         * transform from Level3AssemblyVisualController.
         *
         * Level 2 remains unchanged.
         */
        if (level3VisualController != null)
        {
            assemblyComponent.level3VisualController =
                level3VisualController;

            assemblyComponent.useLevel3SavedTransform =
                true;
        }

        /*
         * REACTIVATE THE COMPONENT ROOT.
         */
        assemblyComponent.gameObject.SetActive(true);

        /*
         * LEVEL 3 PSU ONLY
         *
         * During disassembly, the PSU child objects can become
         * individually inactive.
         *
         * Reactivate only the PSU hierarchy.
         *
         * This block does NOT run for Level 2 or other components.
         */
        if (level3VisualController != null &&
            componentName.Trim().Equals(
                "Power Supply",
                System.StringComparison.OrdinalIgnoreCase))
        {
            Transform[] allChildren =
                assemblyComponent.GetComponentsInChildren<Transform>(true);

            Debug.Log(
                "===== LEVEL 3 PSU VISIBILITY CHECK START ====="
            );

            foreach (Transform child in allChildren)
            {
                if (child == null)
                    continue;

                /*
                 * Reactivate PSU object.
                 */
                child.gameObject.SetActive(true);

                /*
                 * Enable colliders belonging to the PSU.
                 */
                Collider[] childColliders =
                    child.GetComponents<Collider>();

                foreach (Collider collider in childColliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = true;

                        Debug.Log(
                            "PSU COLLIDER | " +
                            child.name +
                            " | colliderEnabled=" +
                            collider.enabled
                        );
                    }
                }

                /*
                 * Enable renderers belonging to the PSU.
                 */
                Renderer[] childRenderers =
                    child.GetComponents<Renderer>();

                foreach (Renderer renderer in childRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;

                        Debug.Log(
                            "PSU RENDERER | " +
                            child.name +
                            " | activeSelf=" +
                            child.gameObject.activeSelf +
                            " | activeInHierarchy=" +
                            child.gameObject.activeInHierarchy +
                            " | rendererEnabled=" +
                            renderer.enabled
                        );
                    }
                }

                /*
                 * Log complete object state.
                 */
                Debug.Log(
                    "PSU OBJECT | " +
                    child.name +
                    " | activeSelf=" +
                    child.gameObject.activeSelf +
                    " | activeInHierarchy=" +
                    child.gameObject.activeInHierarchy +
                    " | position=" +
                    child.position +
                    " | rotation=" +
                    child.rotation.eulerAngles +
                    " | scale=" +
                    child.lossyScale
                );
            }

            Debug.Log(
                "LEVEL 3 PSU: Root, children, colliders and renderers reactivated."
            );

            Debug.Log(
                "===== LEVEL 3 PSU VISIBILITY CHECK END ====="
            );
        }

        /*
         * RE-ENABLE ALL RENDERERS BELONGING TO THIS COMPONENT.
         *
         * This works for both Level 2 and Level 3.
         */
        Renderer[] componentRenderers =
            assemblyComponent.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer
            in componentRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        /*
         * REACTIVATE ADDITIONAL OBJECTS.
         *
         * For the PSU, these should be the cables.
         */
        if (assemblyComponent.additionalObjects != null)
        {
            foreach (GameObject additionalObject
                     in assemblyComponent.additionalObjects)
            {
                if (additionalObject == null)
                    continue;

                additionalObject.SetActive(true);

                Renderer[] additionalRenderers =
                    additionalObject
                        .GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer
                    in additionalRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
            }
        }

        /*
         * START ASSEMBLY.
         *
         * Level 3 PSU uses the manual transform from
         * Level3AssemblyVisualController.
         *
         * Level 2 uses the normal AssemblyComponent behavior.
         */
        assemblyComponent.Assemble();

        /*
         * Disable the drop target after successful placement.
         */
        Collider targetCollider =
            GetComponent<Collider>();

        if (targetCollider != null)
        {
            targetCollider.enabled = false;

            Debug.Log(
                "TARGET COLLIDER DISABLED: " +
                componentName
            );
        }

        Debug.Log(
            "SUCCESSFULLY PLACED: " +
            componentName
        );
    }
}

