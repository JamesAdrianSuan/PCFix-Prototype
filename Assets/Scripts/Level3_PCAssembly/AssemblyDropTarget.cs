using UnityEngine;

public class AssemblyDropTarget : MonoBehaviour
{
    [Header("Target")]
    public string componentName;

    [Header("Assembly Component")]
    public AssemblyComponent assemblyComponent;

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

        // Activate the main component
        assemblyComponent.gameObject.SetActive(true);

        // Activate additional objects / cables
        if (assemblyComponent.additionalObjects != null)
        {
            foreach (GameObject additionalObject
                     in assemblyComponent.additionalObjects)
            {
                if (additionalObject != null)
                {
                    additionalObject.SetActive(true);
                }
            }
        }

        // Assemble component + cables
        assemblyComponent.Assemble();

        // Disable this target collider
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
    }
}