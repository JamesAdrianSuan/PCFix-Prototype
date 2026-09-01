using UnityEngine;
using TMPro;

public class DisassemblyInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string componentName;
        public TMP_Text statusText;

        [HideInInspector]
        public bool completed;
    }

    [Header("Inventory Items")]
    public InventoryItem[] items;

    [Header("Status Symbols")]
    public string incompleteSymbol = "○";
    public string completeSymbol = "✓";

    private void Start()
    {
        ResetInventory();
    }

    // =========================================================
    // RESET INVENTORY
    // =========================================================

    public void ResetInventory()
    {
        if (items == null)
            return;

        foreach (InventoryItem item in items)
        {
            if (item == null)
                continue;

            item.completed = false;
            UpdateItem(item);
        }
    }

    // =========================================================
    // MARK COMPONENT AS REMOVED
    // =========================================================

    public void MarkRemoved(string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            return;

        if (items == null)
            return;

        foreach (InventoryItem item in items)
        {
            if (item == null)
                continue;

            if (string.Equals(
                item.componentName.Trim(),
                componentName.Trim(),
                System.StringComparison.OrdinalIgnoreCase))
            {
                item.completed = true;

                UpdateItem(item);

                Debug.Log(
                    "INVENTORY UPDATED: " +
                    componentName +
                    " = REMOVED"
                );

                return;
            }
        }

        Debug.LogWarning(
            "Inventory item not found: " +
            componentName
        );
    }

    // =========================================================
    // UPDATE ITEM DISPLAY
    // =========================================================

    private void UpdateItem(InventoryItem item)
    {
        if (item.statusText == null)
            return;

        string symbol =
            item.completed
                ? completeSymbol
                : incompleteSymbol;

        item.statusText.text =
            symbol +
            " " +
            item.componentName;
    }
}