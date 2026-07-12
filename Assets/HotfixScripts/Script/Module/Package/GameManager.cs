using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private PackageTable packageTable;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameManager Instance => _instance;

    public PackageTable GetPackageTable()
    {
        if (packageTable == null)
        {
            packageTable = Resources.Load<PackageTable>("UI/TableData/PackageTable");
        }

        return packageTable;
    }

    public PackageTableItem GetPackageItemById(int id)
    {
        return GetPackageTable().GetItemById(id);
    }

    public List<PackageLocalItem> GetPackageLocalData()
    {
        return PackageLocalData.Instance.LoadPackage();
    }

    public PackageLocalItem AddItem(int id, int num)
    {
        PackageTableItem packageTableItem = GetPackageItemById(id);
        if (packageTableItem == null)
        {
            Debug.LogError($"PackageTable 中找不到 id 为 {id} 的物品");
            return null;
        }

        PackageLocalItem packageLocalItem = new PackageLocalItem
        {
            id = packageTableItem.id,
            num = num,
        };

        PackageLocalData.Instance.items.Add(packageLocalItem);
        PackageLocalData.Instance.SavePackage();
        return packageLocalItem;
    }
}
