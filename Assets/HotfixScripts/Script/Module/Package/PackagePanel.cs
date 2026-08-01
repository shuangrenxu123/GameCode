using System.Collections.Generic;
using Character.Player;
using GameSave;
using UIWindow;
using UnityEngine;
using UnityEngine.UI;

public enum PackageMode
{
    normal,
    delete,
    sort,
}
public class PackagePanel : BasePanel
{
    private Transform UIBackground;
    private Transform UIEquipment;
    private Transform UIInventory;
    private Transform UIScrollView;
    private Transform UIContent;
    private Transform UIFrame;
    private Transform UIHeader;
    private Transform UICloseBtn;

    [SerializeField] private PackageCell packageCellPrefab;
    [SerializeField] private PackageTable packageTable;

    private readonly List<PackageCell> cellList = new List<PackageCell>();

    private PlayerInventory inventory;

    override protected void Awake()
    {
        base.Awake();
        InitUI();
    }

    private void Start()
    {
        if (Player.Instance == null)
        {
            Debug.LogError("无法打开背包：场景中不存在 Player");
            return;
        }

        inventory = Player.Instance.Inventory;
        inventory.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshUI;
        }
    }

    private void InitUI()
    {
        InitUIName();
        InitClick();
    }

    private void RefreshUI()
    {
        if (inventory == null)
        {
            return;
        }

        RefreshScroll();
    }

    private void InitUIName()
    {
        UIBackground = transform.Find("Background");
        UIEquipment = transform.Find("Equipment");
        UIInventory = transform.Find("Inventory");
        UIScrollView = transform.Find("Inventory/Scroll View");
        UIContent = transform.Find("Inventory/Scroll View/Viewport/Content");
        UIFrame = transform.Find("Footer");
        UIHeader = transform.Find("Header");
        UICloseBtn = transform.Find("CloseButton");
    }

    private void InitClick()
    {
        UICloseBtn.GetComponent<Button>().onClick.AddListener(OnClickClose);
    }
    
    private void OnClickClose()
    {
        print(">>>>> OnClickClose");
        ClosePanel();
    }

    private void RefreshScroll()
    {
        RectTransform scrollContent = UIScrollView.GetComponent<ScrollRect>().content;
        if (packageTable == null)
        {
            packageTable = Resources.Load<PackageTable>("UI/TableData/PackageTable");
        }

        ClearCells(scrollContent);
        IReadOnlyList<InventoryItemSaveData> items = inventory.Items;
        for (int i = 0; i < items.Count; i++)
        {
            PackageCell packageCell = Instantiate(packageCellPrefab, scrollContent);
            packageCell.transform.SetSiblingIndex(i);
            packageCell.Refresh(items[i], packageTable);
            cellList.Add(packageCell);
        }
    }

    private void ClearCells(RectTransform scrollContent)
    {
        if (cellList.Count == 0)
        {
            PackageCell[] existingCells = scrollContent.GetComponentsInChildren<PackageCell>(true);
            cellList.AddRange(existingCells);
        }

        for (int i = 0; i < cellList.Count; i++)
        {
            PackageCell cell = cellList[i];
            if (cell != null)
            {
                Destroy(cell.gameObject);
            }
        }

        cellList.Clear();
    }
}
