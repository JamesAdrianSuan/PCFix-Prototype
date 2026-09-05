using UnityEngine;
using TMPro;

public class DisassemblyInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string componentName;
        public Sprite componentIcon;
        public TMP_Text statusText;
        public InventorySlot inventorySlot;

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

    public void ResetInventory()
    {
        if (items == null)
            return;

        foreach (InventoryItem item in items)
        {
            if (item == null)
                continue;

            item.completed = false;

            if (item.inventorySlot != null)
                item.inventorySlot.gameObject.SetActive(false);

            UpdateItem(item);
        }
    }

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

                // SHOW INVENTORY PICTURE
                if (item.inventorySlot != null)
                {
                    item.inventorySlot.gameObject.SetActive(true);

                    item.inventorySlot.Setup(
                        item.componentName,
                        item.componentIcon,
                        this
                    );
                }

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

    public void SelectItem(string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            return;

        Debug.Log(
            "INVENTORY ITEM SELECTED: " +
            componentName
        );
    }
}