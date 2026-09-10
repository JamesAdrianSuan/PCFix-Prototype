using UnityEngine;
using UnityEngine.InputSystem;

public class AssemblyCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera")]
    public float distance = 6f;

    [Tooltip("Closest the camera can get to the system unit.")]
    public float minDistance = 1.8f;

    [Tooltip("Farthest the camera can move from the system unit.")]
    public float maxDistance = 9f;

    [Header("Zoom")]
    public float zoomSpeed = 0.15f;

    [Tooltip("How smoothly the camera reaches the zoom distance.")]
    public float zoomSmoothness = 12f;

    [Header("Rotation")]
    public float rotationSpeed = 0.15f;

    [Header("Pan / Hand Tool")]
    public float panSpeed = 0.01f;

    [Tooltip("How smoothly the camera follows the pan movement.")]
    public float panSmoothness = 12f;

    [Header("Target Height")]
    public float targetHeight = 1f;

    private float yaw;
    private float pitch;

    private float targetDistance;

    private Vector3 panOffset;
    private Vector3 targetPanOffset;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning(
                "AssemblyCameraController: Target is not assigned."
            );

            return;
        }

        // Use the camera's current rotation as the starting view.
        Vector3 startingAngles =
            transform.eulerAngles;

        yaw = startingAngles.y;
        pitch = startingAngles.x;

        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        // Calculate current distance from target.
        Vector3 lookTarget =
            target.position +
            Vector3.up * targetHeight;

        distance =
            Vector3.Distance(
                transform.position,
                lookTarget
            );

        distance =
            Mathf.Clamp(
                distance,
                minDistance,
                maxDistance
            );

        targetDistance = distance;

        panOffset = Vector3.zero;
        targetPanOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // =========================================
        // RIGHT MOUSE BUTTON = ROTATE
        // =========================================

        if (
            Mouse.current != null &&
            Mouse.current.rightButton.isPressed
        )
        {
            Vector2 mouseDelta =
                Mouse.current.delta.ReadValue();

            yaw +=
                mouseDelta.x *
                rotationSpeed;

            pitch -=
                mouseDelta.y *
                rotationSpeed;

            pitch =
                Mathf.Clamp(
                    pitch,
                    -10f,
                    70f
                );
        }

        // =========================================
        // MIDDLE MOUSE BUTTON = HAND / PAN
        // =========================================

        if (
            Mouse.current != null &&
            Mouse.current.middleButton.isPressed
        )
        {
            Vector2 mouseDelta =
                Mouse.current.delta.ReadValue();

            Vector3 panMovement =
                (-transform.right *
                 mouseDelta.x +
                 -transform.up *
                 mouseDelta.y) *
                panSpeed;

            targetPanOffset += panMovement;
        }

        // =========================================
        // MOUSE WHEEL = ZOOM
        // =========================================

        if (Mouse.current != null)
        {
            float scroll =
                Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetDistance -=
                    scroll *
                    zoomSpeed;

                targetDistance =
                    Mathf.Clamp(
                        targetDistance,
                        minDistance,
                        maxDistance
                    );
            }
        }

        // =========================================
        // SMOOTH ZOOM
        // =========================================

        distance =
            Mathf.Lerp(
                distance,
                targetDistance,
                zoomSmoothness *
                Time.deltaTime
            );

        // =========================================
        // SMOOTH PAN
        // =========================================

        panOffset =
            Vector3.Lerp(
                panOffset,
                targetPanOffset,
                panSmoothness *
                Time.deltaTime
            );

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        Vector3 lookTarget =
            target.position +
            Vector3.up * targetHeight +
            panOffset;

        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        transform.position =
            lookTarget -
            rotation *
            Vector3.forward *
            distance;

        transform.LookAt(lookTarget);
    }
}