using UnityEngine;
using System.Collections;

public class DisassemblyCable : MonoBehaviour
{
    [Header("Cable Settings")]
    public string cableGroup = "PSU";

    [Header("Additional Objects")]
    [Tooltip("Independent objects that belong to THIS cable and should disappear with it.")]
    public GameObject[] additionalObjects;

    [Header("Shared Connector")]
    [Tooltip("A connector shared by multiple cables. It will only disappear when all required cables are disconnected.")]
    public GameObject sharedConnector;

    [Tooltip("Other DisassemblyCable objects that must also be disconnected before the shared connector disappears.")]
    public DisassemblyCable[] connectorRequiredCables;

    [Header("Detach Animation")]
    public float detachDuration = 0.5f;

    [Tooltip("How far the cable moves away before disappearing.")]
    public float detachDistance = 0.15f;

    private DisassemblyManager manager;

    private bool interactable = false;
    private bool isDetaching = false;
    private bool hasBeenDisconnected = false;

    private void Awake()
    {
        SetupAdditionalObjectClickForwarders();
    }

    public void Setup(DisassemblyManager disassemblyManager)
    {
        manager = disassemblyManager;

        SetupAdditionalObjectClickForwarders();
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    public void SetInteractable(bool value)
    {
        interactable = value;

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = value;
        }

        // Additional objects are part of this cable assembly.
        if (additionalObjects != null)
        {
            foreach (GameObject additionalObject in additionalObjects)
            {
                if (additionalObject == null)
                    continue;

                Collider[] additionalColliders =
                    additionalObject.GetComponentsInChildren<Collider>(true);

                foreach (Collider col in additionalColliders)
                {
                    col.enabled = value;
                }
            }
        }
    }

    // =========================================================
    // ADDITIONAL OBJECT CLICK FORWARDERS
    // =========================================================

    private void SetupAdditionalObjectClickForwarders()
    {
        if (additionalObjects == null)
            return;

        foreach (GameObject additionalObject in additionalObjects)
        {
            if (additionalObject == null)
                continue;

            Collider[] colliders =
                additionalObject.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                ChildCableClickForwarder forwarder =
                    col.GetComponent<ChildCableClickForwarder>();

                if (forwarder == null)
                {
                    forwarder =
                        col.gameObject.AddComponent<
                            ChildCableClickForwarder>();
                }

                forwarder.SetParentCable(this);
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
        if (!interactable || isDetaching || hasBeenDisconnected)
            return;

        if (manager == null)
        {
            Debug.LogError(
                "Cable has NO DisassemblyManager: " +
                gameObject.name
            );

            return;
        }

        Debug.Log(
            "CABLE CLICKED: " +
            gameObject.name +
            " | Group: " +
            cableGroup
        );

        manager.CableClicked(this);
    }

    // =========================================================
    // DISCONNECT STATE
    // =========================================================

    public bool HasBeenDisconnected()
    {
        return hasBeenDisconnected;
    }

    // =========================================================
    // DETACH
    // =========================================================

    public void Detach()
    {
        if (isDetaching || hasBeenDisconnected)
            return;

        hasBeenDisconnected = true;

        StartCoroutine(DetachRoutine());
    }

    // =========================================================
    // DETACH ROUTINE
    // =========================================================

    private IEnumerator DetachRoutine()
    {
        isDetaching = true;

        // Stop the cable from being clicked again.
        SetInteractable(false);

        // =====================================================
        // MAIN CABLE
        // =====================================================

        Vector3 startPosition =
            transform.position;

        Vector3 startScale =
            transform.localScale;

        Vector3 direction =
            transform.forward;

        Vector3 targetPosition =
            startPosition +
            direction * detachDistance;

        // =====================================================
        // ADDITIONAL OBJECTS
        // =====================================================

        int objectCount =
            additionalObjects != null
                ? additionalObjects.Length
                : 0;

        Vector3[] startPositions =
            new Vector3[objectCount];

        Vector3[] startScales =
            new Vector3[objectCount];

        Vector3[] targetPositions =
            new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            GameObject additionalObject =
                additionalObjects[i];

            if (additionalObject == null)
                continue;

            startPositions[i] =
                additionalObject.transform.position;

            startScales[i] =
                additionalObject.transform.localScale;

            targetPositions[i] =
                startPositions[i] +
                direction * detachDistance;
        }

        // =====================================================
        // ANIMATION
        // =====================================================

        float elapsed = 0f;

        while (elapsed < detachDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed / detachDuration;

            t = Mathf.Clamp01(t);

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            // -------------------------------------------------
            // MAIN CABLE
            // -------------------------------------------------

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    t
                );

            // -------------------------------------------------
            // ADDITIONAL OBJECTS
            // -------------------------------------------------

            for (int i = 0; i < objectCount; i++)
            {
                GameObject additionalObject =
                    additionalObjects[i];

                if (additionalObject == null)
                    continue;

                additionalObject.transform.position =
                    Vector3.Lerp(
                        startPositions[i],
                        targetPositions[i],
                        t
                    );

                additionalObject.transform.localScale =
                    Vector3.Lerp(
                        startScales[i],
                        Vector3.zero,
                        t
                    );
            }

            yield return null;
        }

        // =====================================================
        // FINAL STATE
        // =====================================================

        transform.position =
            targetPosition;

        transform.localScale =
            Vector3.zero;

        for (int i = 0; i < objectCount; i++)
        {
            GameObject additionalObject =
                additionalObjects[i];

            if (additionalObject == null)
                continue;

            additionalObject.transform.position =
                targetPositions[i];

            additionalObject.transform.localScale =
                Vector3.zero;

            Debug.Log(
                "CABLE ADDITIONAL OBJECT REMOVED: " +
                additionalObject.name
            );
        }

        Debug.Log(
            "CABLE REMOVED: " +
            gameObject.name
        );

        // =====================================================
        // SHARED CONNECTOR
        // =====================================================

        TryRemoveSharedConnector();

        isDetaching = false;
    }

    // =========================================================
    // SHARED CONNECTOR LOGIC
    // =========================================================

    private void TryRemoveSharedConnector()
    {
        if (sharedConnector == null)
            return;

        // Check every cable that is required for this
        // connector to disappear.
        if (connectorRequiredCables != null)
        {
            foreach (
                DisassemblyCable requiredCable
                in connectorRequiredCables)
            {
                if (requiredCable == null)
                    continue;

                if (!requiredCable.HasBeenDisconnected())
                {
                    Debug.Log(
                        "Shared connector remains because cable is " +
                        "still connected: " +
                        requiredCable.gameObject.name
                    );

                    return;
                }
            }
        }

        StartCoroutine(
            RemoveSharedConnectorRoutine()
        );
    }

    private IEnumerator RemoveSharedConnectorRoutine()
    {
        Vector3 startPosition =
            sharedConnector.transform.position;

        Vector3 startScale =
            sharedConnector.transform.localScale;

        Vector3 direction =
            sharedConnector.transform.forward;

        Vector3 targetPosition =
            startPosition +
            direction * detachDistance;

        float elapsed = 0f;

        while (elapsed < detachDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed / detachDuration;

            t = Mathf.Clamp01(t);

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            sharedConnector.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            sharedConnector.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    t
                );

            yield return null;
        }

        sharedConnector.transform.position =
            targetPosition;

        sharedConnector.transform.localScale =
            Vector3.zero;

        Collider[] colliders =
            sharedConnector.GetComponentsInChildren<Collider>(
                true
            );

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Debug.Log(
            "SHARED CONNECTOR REMOVED: " +
            sharedConnector.name
        );
    }
}


// =============================================================
// CHILD CABLE CLICK FORWARDER
// =============================================================

public class ChildCableClickForwarder : MonoBehaviour
{
    private DisassemblyCable parentCable;

    public void SetParentCable(
        DisassemblyCable cable)
    {
        parentCable = cable;
    }

    private void OnMouseDown()
    {
        if (parentCable != null)
        {
            parentCable.HandleClick();
        }
    }
}