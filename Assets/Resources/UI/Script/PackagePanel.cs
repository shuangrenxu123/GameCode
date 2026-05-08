using System.Collections;
using System.Collections.Generic;
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

    private int lastTotalNum = -1;

    override protected void Awake()
    {
        base.Awake();
        InitUI();
    }

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (PackageLocalData.Instance == null) return;
        if (PackageLocalData.Instance.items == null) return;

        int currentTotalNum = 0;

        foreach (PackageLocalItem item in PackageLocalData.Instance.items)
        {
            currentTotalNum += item.num;
        }

        if (currentTotalNum != lastTotalNum)
        {
            lastTotalNum = currentTotalNum;
            RefreshUI();
        }
    }

    private void InitUI()
    {
        InitUIName();
        InitClick();
    }

    private void RefreshUI()
    {
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

        List<PackageLocalItem> localDataList = GameManager.Instance.GetPackageLocalData();

        int itemCount = localDataList.Count;

        // 删除前 itemCount 个格子
        for (int i = 0; i < itemCount; i++)
        {
            if (scrollContent.childCount <= 0)
            {
                Debug.LogWarning("背包格子不够删了");
                break;
            }

            Transform oldCell = scrollContent.GetChild(0);

            // 先从 Content 里移出去，再 Destroy
            oldCell.SetParent(null);
            Destroy(oldCell.gameObject);
        }

        // 把本地背包数据生成到最前面
        for (int i = 0; i < itemCount; i++)
        {
            Transform packageUIItem = Instantiate(packageCellPrefab.transform, scrollContent);

            // 插到最前面的第 i 个位置
            packageUIItem.SetSiblingIndex(i);

            PackageCell packageCell = packageUIItem.GetComponent<PackageCell>();
            packageCell.Refresh(localDataList[i], this);
        }
    }
}
