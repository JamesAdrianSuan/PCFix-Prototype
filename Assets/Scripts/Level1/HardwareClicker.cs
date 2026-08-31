using UnityEngine;
using UnityEngine.InputSystem;

public class HardwareClicker : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    private HardwareComponent selectedComponent;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        // New Unity Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectHardware();
        }
    }

    private void SelectHardware()
    {
        if (playerCamera == null)
        {
            Debug.LogError("HardwareClicker: No camera found.");
            return;
        }

        // Get current mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Create a ray from the camera through the mouse position
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        // Check what the ray hits
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HardwareComponent component =
                hit.collider.GetComponent<HardwareComponent>();

            if (component != null)
            {
                SelectComponent(component);
            }
        }
    }

    private void SelectComponent(HardwareComponent component)
    {
        // Remove highlight from previous component
        if (selectedComponent != null)
        {
            selectedComponent.RemoveHighlight();
        }

        // Store selected component
        selectedComponent = component;

        // Highlight selected component
        selectedComponent.Highlight();

        // Display information in Console
        Debug.Log(
            "Selected Hardware: " +
            selectedComponent.GetName()
        );

        Debug.Log(
            "Description: " +
            selectedComponent.GetDescription()
        );
    }
}