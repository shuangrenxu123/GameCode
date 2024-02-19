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
    /// ����Ʒ
    /// </summary>
    Consumable,
    /// <summary>
    /// װ��
    /// </summary>
    Equip,
    /// <summary>
    /// ����
    /// </summary>
    Weapon,
    /// <summary>
    /// ����
    /// </summary>
    other
}
