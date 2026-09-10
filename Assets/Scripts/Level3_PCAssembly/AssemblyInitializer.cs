using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level3AssemblyInitializer : MonoBehaviour
{
    [System.Serializable]
    public class AssemblyStartItem
    {
        public string componentName;
        public GameObject componentObject;
    }

    [Header("Inventory")]
    public DisassemblyInventory inventory;

    [Header("Components")]
    public AssemblyStartItem[] components;

    [Header("Inventory Randomization")]
    public bool randomizeInventory = true;

    private void Start()
    {
        StartCoroutine(InitializeLevel3());
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private IEnumerator InitializeLevel3()
    {
        // Wait one frame so all components have initialized.
        yield return null;

        HideAllComponents();

        if (randomizeInventory)
        {
            ShuffleInventorySlots();
        }

        PopulateInventory();

        Debug.Log(
            "=== LEVEL 3 ASSEMBLY STARTING STATE READY ==="
        );
    }

    // =========================================================
    // HIDE ALL ASSEMBLY COMPONENTS
    // =========================================================

    private void HideAllComponents()
    {
        if (components == null)
            return;

        foreach (AssemblyStartItem item in components)
        {
            if (item == null)
                continue;

            if (item.componentObject == null)
                continue;

            // Hide main component
            item.componentObject.SetActive(false);

            AssemblyComponent assemblyComponent =
                item.componentObject.GetComponent<
                    AssemblyComponent>();

            if (assemblyComponent != null &&
                assemblyComponent.additionalObjects != null)
            {
                foreach (
                    GameObject additionalObject
                    in assemblyComponent.additionalObjects)
                {
                    if (additionalObject != null)
                    {
                        additionalObject.SetActive(false);
                    }
                }
            }

            Debug.Log(
                "LEVEL 3 HIDDEN: " +
                item.componentName
            );
        }
    }

    // =========================================================
    // RANDOMIZE INVENTORY SLOTS
    // =========================================================

    private void ShuffleInventorySlots()
    {
        if (inventory == null)
        {
            Debug.LogError(
                "LEVEL 3 INVENTORY IS NULL!"
            );

            return;
        }

        if (inventory.items == null ||
            inventory.items.Length == 0)
        {
            Debug.LogWarning(
                "LEVEL 3 INVENTORY HAS NO ITEMS!"
            );

            return;
        }

        List<Transform> slotTransforms =
            new List<Transform>();

        // Collect physical inventory slots
        foreach (
            DisassemblyInventory.InventoryItem item
            in inventory.items)
        {
            if (item == null)
                continue;

            if (item.inventorySlot == null)
                continue;

            slotTransforms.Add(
                item.inventorySlot.transform
            );
        }

        if (slotTransforms.Count <= 1)
            return;

        // Fisher-Yates shuffle
        for (int i = slotTransforms.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            Transform temp =
                slotTransforms[i];

            slotTransforms[i] =
                slotTransforms[randomIndex];

            slotTransforms[randomIndex] =
                temp;
        }

        // Change only their visual/UI positions
        for (int i = 0; i < slotTransforms.Count; i++)
        {
            slotTransforms[i].SetSiblingIndex(i);
        }

        Debug.Log(
            "=== LEVEL 3 INVENTORY POSITIONS RANDOMIZED ==="
        );
    }

    private void PopulateInventory()
    {
        if (inventory == null)
        {
            Debug.LogError(
                "LEVEL 3 INVENTORY IS NULL!"
            );

            return;
        }

        if (components == null)
            return;

        foreach (AssemblyStartItem item in components)
        {
            if (item == null)
                continue;

            if (string.IsNullOrWhiteSpace(
                item.componentName))
                continue;

            inventory.MarkRemoved(
                item.componentName
            );
        }
    }
}