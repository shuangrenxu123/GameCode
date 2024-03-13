using UnityEngine;
using Utilities;
using static Utilities.ConditionAttribute;
[CreateAssetMenu(menuName = "Items/EquipItemData")]
public class EquipItemData : ItemData
{
    public override ItemType Type { get => ItemType.Equip; }
    public EquipType equipType;
    public GameObject modelPrefab;

    public Mesh mesh;

    public void Equip(PlayerInventory equipmanager)
    {
        equipmanager.ReplaceEquipe(equipType,mesh);
    }
}
