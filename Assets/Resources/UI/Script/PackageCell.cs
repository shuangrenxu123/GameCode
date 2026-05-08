using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackageCell : MonoBehaviour
{
    private Transform UIBackground;
    private Transform UIIcon;
    private Transform UIFrame;
    private Transform UIAmount;

    private PackageLocalItem packageLocalData;
    private PackageTableItem packageTableItem;
    private PackageTable packageTableSO;
    private PackagePanel uiParent;

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

    public void Refresh(PackageLocalItem packageLocalData, PackagePanel uiParent)
    {
        // 数据初始化
        this.packageLocalData = packageLocalData;
        this.packageTableItem = GameManager.Instance.GetPackageItemById(packageLocalData.id);
        this.uiParent = uiParent;
        // 物品的图片
        UIIcon.GetComponent<Image>().sprite = packageTableItem.Icon;

        //物品的数量
        UIAmount.GetComponent<TMP_Text>().text = packageLocalData.num.ToString();
    }
}
