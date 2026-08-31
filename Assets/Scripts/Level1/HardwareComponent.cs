using UnityEngine;

public class HardwareComponent : MonoBehaviour
{
    [Header("Hardware Information")]
    public string componentName;

    [TextArea(2, 5)]
    public string description;

    private Renderer componentRenderer;
    private Color originalColor;

    private void Awake()
    {
        componentRenderer = GetComponent<Renderer>();

        if (componentRenderer != null)
        {
            originalColor = componentRenderer.material.color;
        }
    }

    public void Highlight()
    {
        if (componentRenderer != null)
        {
            componentRenderer.material.color = Color.yellow;
        }
    }

    public void RemoveHighlight()
    {
        if (componentRenderer != null)
        {
            componentRenderer.material.color = originalColor;
        }
    }

    public string GetName()
    {
        return componentName;
    }

    public string GetDescription()
    {
        return description;
    }
}