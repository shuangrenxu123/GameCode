using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public override ItemType Type { get => ItemType.Weapon; }
    public WeaponType WeaponType;
    public GameObject modle;
}
