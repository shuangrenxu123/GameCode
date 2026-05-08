using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BeiBao/PackageTable", fileName = "PackageTable")]

public class PackageTable : ScriptableObject
{
    public List<PackageTableItem> DataList = new List<PackageTableItem>();

    public PackageTableItem GetItemById(int id)
    {
        return DataList.Find(item => item.id == id);
    }
}

public enum ItemTypes
{
    Weapon,
    Consumable,
    Armor,
    Material
}

[System.Serializable]
public class PackageTableItem
{
    public int id; 
    public ItemTypes type;
    public string name;
    public string description;
    public string skillDescription;
    public Sprite Icon;
}