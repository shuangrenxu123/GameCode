using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Items/EquipItemData")]
public class EquipItemData : ItemData
{
    public EquipType equipType { get; private set;}
    public GameObject modelPrefab { get; private set; }
    public Mesh mesh { get; private set; }
}
