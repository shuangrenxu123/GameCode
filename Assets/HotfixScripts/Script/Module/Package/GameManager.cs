using Character.Player;
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

    public bool AddItem(int id, int num)
    {
        PackageTableItem packageTableItem = GetPackageItemById(id);
        if (packageTableItem == null)
        {
            Debug.LogError($"PackageTable 中找不到 id 为 {id} 的物品");
            return false;
        }

        if (Player.Instance == null)
        {
            Debug.LogError("场景中不存在 Player，无法添加背包物品");
            return false;
        }

        bool stackable = packageTableItem.type != ItemTypes.Weapon && packageTableItem.type != ItemTypes.Armor;
        Player.Instance.Inventory.AddItem(id, num, stackable);
        return true;
    }
}
