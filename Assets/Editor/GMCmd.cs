using System;
using System.Collections;
using System.Collections.Generic;
using UIWindow;
using UnityEditor;
using UnityEngine;

public class GMCmd : ScriptableWizard
{
    public int id = 1;
    public int num = 1;

    [MenuItem("CMCmd/读取表格")]
    public static void ReadTable()
    {
        PackageTable packageTable = Resources.Load<PackageTable>("UI/TableData/PackageTable");
        foreach (PackageTableItem packageItem in packageTable.DataList)
        {
            Debug.Log(string.Format("【id】:{0}, 【name】:{1}", packageItem.id, packageItem.name));
        }
    }

    [MenuItem("CMCmd/创建背包测试数据")]
    public static void CreateLocalPackageData()
    {
        // 保存数据
        PackageLocalData.Instance.items = new List<PackageLocalItem>();
        PackageLocalData.Instance.SavePackage();
    }

    [MenuItem("CMCmd/读取背包测试数据")]
    public static void ReadLocalPackageData()
    {
        // 读取数据
        List<PackageLocalItem> readItems = PackageLocalData.Instance.LoadPackage();
        foreach (PackageLocalItem item in readItems)
        {
            Debug.Log(item);
        }
    }

    [MenuItem("CMCmd/打开背包主界面")]
    public static void OpenPackagePanel()
    {
        AUIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    [MenuItem("CMCmd/添加物品")]
    private static void OpenAddItemWindow()
    {
        DisplayWizard<GMCmd>("添加物品", "添加");
    }

    private void OnWizardCreate()
    {
        GameManager.Instance.AddItem(id, num);
    }
}
