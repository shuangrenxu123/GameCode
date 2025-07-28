using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色背包管理器
/// </summary>
public class PlayerInventory
{
    Dictionary<Guid, ItemData> bagSlot;
    #region Event 

    public event Action<ItemData, int> OnItemAdd;
    public event Action<ItemData, int> OnItemRemove;

    #endregion

    /// <summary>
    /// 添加道具
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(ItemData item, int count = 1)
    {

    }
    public void DropItem(Guid guid, int count = 1)
    {
    }
    public void DropItem(int id, int count = 1)
    {

    }
}
