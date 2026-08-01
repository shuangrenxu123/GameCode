using GameSave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageCell : MonoBehaviour
{
    private Transform UIBackground;
    private Transform UIIcon;
    private Transform UIFrame;
    private Transform UIAmount;

    private PackageTableItem packageTableItem;

    public InventoryItemSaveData Item { get; private set; }

    private void Awake()
    {
        InitUIName();
    }
    private void InitUIName()
    {
        UIBackground = transform.Find("Background");
        UIIcon = transform.Find("Item/ICON");
        UIFrame = transform.Find("Frame");
        UIAmount = transform.Find("Label_Amount");
    }

    public void Refresh(InventoryItemSaveData inventoryItem, PackageTable packageTable)
    {
        Item = inventoryItem;
        packageTableItem = packageTable != null ? packageTable.GetItemById(inventoryItem.itemId) : null;

        Image icon = UIIcon.GetComponent<Image>();
        icon.sprite = packageTableItem != null ? packageTableItem.Icon : null;
        icon.enabled = icon.sprite != null;
        UIAmount.GetComponent<TMP_Text>().text = inventoryItem.count > 1
            ? inventoryItem.count.ToString()
            : string.Empty;
    }
}
