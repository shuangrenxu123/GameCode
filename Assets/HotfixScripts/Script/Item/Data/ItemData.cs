using UnityEngine;
[CreateAssetMenu(menuName = "Items/TestItem")]
public class ItemData : ScriptableObject
{
    public int id;
    public Sprite Icon;
    public string Name;
    public string Description;
    public ItemType Type;

}
public enum ItemType
{
    /// <summary>
    /// 消耗品
    /// </summary>
    Consumable,
    /// <summary>
    /// 装备
    /// </summary>
    Equip,
    /// <summary>
    /// 武器
    /// </summary>
    Weapon,
    /// <summary>
    /// 其他
    /// </summary>
    other
}
