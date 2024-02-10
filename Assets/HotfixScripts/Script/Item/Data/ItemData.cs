using UnityEngine;

public class ItemData : ScriptableObject
{
    public int id { get; private set; }
    public Sprite Icon { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ItemType Type { get; private set; }

}
public enum ItemType
{
    Consumable,
    Equip,
    other
}
public enum EquipItemType
{
    Weapon,
    Equipe
}
