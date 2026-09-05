using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("UI")]
    public Image componentImage;
    public TMP_Text componentNameText;
    public Button button;

    private string componentName;
    private DisassemblyInventory inventory;

    public void Setup(
        string name,
        Sprite icon,
        DisassemblyInventory inventoryReference)
    {
        componentName = name;
        inventory = inventoryReference;

        if (componentImage != null)
        {
            componentImage.sprite = icon;
            componentImage.enabled = icon != null;
        }

        if (componentNameText != null)
            componentNameText.text = name;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (inventory != null)
            inventory.SelectItem(componentName);
    }
}