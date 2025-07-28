using UnityEngine;
[CreateAssetMenu(menuName = "Items/EquipItemData")]
public class EquipItemData : ItemData
{
    public override ItemType Type { get => ItemType.Equip; }
    public EquipType equipType;
    public Mesh mesh;
}
