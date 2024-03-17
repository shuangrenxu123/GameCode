using UnityEngine;
public class ItemData : ScriptableObject
{
    public int id;
    public Sprite Icon;
    public string Name;
    public string Description;
    public virtual ItemType Type { get; }

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
    Equip=2,
    /// <summary>
    /// 武器
    /// </summary>
    Weapon=3,
    /// <summary>
    /// 其他
    /// </summary>
    other
}
