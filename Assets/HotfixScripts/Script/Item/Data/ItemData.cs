using System;
using UnityEngine;
[Serializable]
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
    /// ����Ʒ
    /// </summary>
    Consumable,
    /// <summary>
    /// װ��
    /// </summary>
    Equip = 2,
    /// <summary>
    /// ����
    /// </summary>
    Weapon = 3,
    /// <summary>
    /// ����
    /// </summary>
    other
}
