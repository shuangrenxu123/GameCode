using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public override ItemType Type { get => ItemType.Weapon; }
    public GameObject modle;
}
