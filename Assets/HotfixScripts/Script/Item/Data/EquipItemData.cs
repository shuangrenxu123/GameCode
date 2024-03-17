using UnityEngine;
[CreateAssetMenu(menuName = "Items/EquipItemData")]
public class EquipItemData : ItemData
{
    public override ItemType Type { get => ItemType.Equip; }
    public EquipType equipType;
    public Mesh mesh;

    public void Equip(PlayerInventory equipmanager)
    {
        equipmanager.ReplaceEquipe(equipType,mesh);
    }
}
