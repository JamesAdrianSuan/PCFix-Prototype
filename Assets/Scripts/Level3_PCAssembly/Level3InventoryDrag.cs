using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Level3InventoryDrag :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Component")]
    public string componentName;

    [Header("Inventory Image")]
    public Image componentImage;

    [Header("Drag Settings")]
    public float dragAlpha = 0.8f;
    public Vector2 dragSize = new Vector2(120f, 120f);

    private Canvas canvas;
    private AssemblyManager assemblyManager;

    private GameObject dragObject;
    private RectTransform dragRect;
    private CanvasGroup dragCanvasGroup;

    private void Awake()
    {
        canvas =
            GetComponentInParent<Canvas>();

        assemblyManager =
    FindAnyObjectByType<AssemblyManager>();
    }

    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(componentName))
        {
            Debug.LogWarning(
                "Level3InventoryDrag has no component name: " +
                gameObject.name
            );

            return;
        }

        if (componentImage == null ||
            componentImage.sprite == null)
        {
            Debug.LogWarning(
                "No inventory image assigned: " +
                componentName
            );

            return;
        }

        if (canvas == null)
        {
            canvas =
                GetComponentInParent<Canvas>();
        }

        if (assemblyManager == null)
        {
            assemblyManager =
    FindAnyObjectByType<AssemblyManager>();
        }

        if (canvas == null)
        {
            Debug.LogError(
                "No Canvas found for Level3InventoryDrag!"
            );

            return;
        }

        if (assemblyManager == null)
        {
            Debug.LogError(
                "No AssemblyManager found in Level 3!"
            );

            return;
        }

        Debug.Log(
            "LEVEL 3 START DRAG: " +
            componentName
        );

        CreateDragObject(
            eventData.position
        );
    }

    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (dragRect == null)
            return;

        dragRect.position =
            eventData.position;
    }

    // =========================================================
    // END DRAG
    // =========================================================

    public void OnEndDrag(
        PointerEventData eventData)
    {
        Debug.Log(
            "LEVEL 3 END DRAG: " +
            componentName
        );

        TryDropOnAssemblyTarget(
            eventData
        );

        DestroyDragObject();
    }

    // =========================================================
    // CREATE DRAG IMAGE
    // =========================================================

    private void CreateDragObject(
        Vector2 screenPosition)
    {
        dragObject =
            new GameObject(
                "Level3_Drag_" +
                componentName
            );

        dragObject.transform.SetParent(
            canvas.transform,
            false
        );

        dragRect =
            dragObject.AddComponent<
                RectTransform>();

        dragRect.sizeDelta =
            dragSize;

        Image image =
            dragObject.AddComponent<Image>();

        image.sprite =
            componentImage.sprite;

        image.preserveAspect = true;

        dragCanvasGroup =
            dragObject.AddComponent<
                CanvasGroup>();

        dragCanvasGroup.alpha =
            dragAlpha;

        // Prevent drag image from blocking
        // the 3D PC raycast.
        dragCanvasGroup.blocksRaycasts =
            false;

        dragRect.position =
            screenPosition;
    }

    // =========================================================
    // CHECK DROP
    // =========================================================

    private void TryDropOnAssemblyTarget(
        PointerEventData eventData)
    {
        if (assemblyManager == null)
        {
            assemblyManager =
       FindAnyObjectByType<AssemblyManager>();
        }

        if (assemblyManager == null)
        {
            Debug.LogError(
                "LEVEL 3: AssemblyManager not found!"
            );

            return;
        }

        Camera cam =
            Camera.main;

        if (cam == null)
        {
            Debug.LogError(
                "LEVEL 3: Main Camera not found!"
            );

            return;
        }

        // -----------------------------------------------------
        // CHECK CURRENT ASSEMBLY STEP FIRST
        // -----------------------------------------------------

        if (!assemblyManager.IsCorrectAssemblyStep(
            componentName))
        {
            string expected =
                assemblyManager.GetCurrentExpectedComponent();

            assemblyManager.RecordMistake(
                "Wrong component! Install the " +
                expected
            );

            Debug.Log(
                "LEVEL 3 WRONG STEP | " +
                "Dragged: " +
                componentName +
                " | Expected: " +
                expected
            );

            return;
        }
        // -----------------------------------------------------
        // RAYCAST
        // -----------------------------------------------------

        Ray ray =
            cam.ScreenPointToRay(
                eventData.position
            );

        RaycastHit hit;

        if (!Physics.Raycast(
     ray,
     out hit,
     1000f))
        {
            assemblyManager.RecordMistake(
                "Drop missed the computer."
            );

            Debug.Log(
                "LEVEL 3: Drop missed the PC."
            );

            return;
        }

        AssemblyDropTarget target =
            hit.collider.GetComponent<
                AssemblyDropTarget>();

        if (target == null)
        {
            target =
                hit.collider.GetComponentInParent<
                    AssemblyDropTarget>();
        }
        if (target == null)
        {
            assemblyManager.RecordMistake(
                "Wrong location! Drop the component in the correct area."
            );

            Debug.Log(
                "LEVEL 3: Drop was not on an " +
                "AssemblyDropTarget."
            );

            return;
        }

        Debug.Log(
            "LEVEL 3 DROP | Dragged: " +
            componentName +
            " | Target: " +
            target.componentName
        );

        // -----------------------------------------------------
        // CHECK CORRECT TARGET
        // -----------------------------------------------------

        if (!target.IsCorrectComponent(
     componentName))
        {
            assemblyManager.RecordMistake(
                "Wrong location! Place the " +
                componentName +
                " in the correct area."
            );

            Debug.Log(
                "LEVEL 3 WRONG LOCATION: " +
                componentName
            );

            return;
        }

        // -----------------------------------------------------
        // CORRECT
        // -----------------------------------------------------

        Debug.Log(
            "LEVEL 3 CORRECT ASSEMBLY: " +
            componentName
        );

        bool completed =
            assemblyManager.CompleteFromInventory(
                componentName,
                target
            );

        if (completed)
        {
            // Hide inventory slot after successful placement.
            gameObject.SetActive(false);
        }
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void DestroyDragObject()
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
        }

        dragObject = null;
        dragRect = null;
        dragCanvasGroup = null;
    }
}