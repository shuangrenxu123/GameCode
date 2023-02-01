using System.Collections.Generic;
using UnityEngine.Events;

public class BagSystem
{
    public Dictionary<int, ItemList> Items;
    /// <summary>
    /// 背包最大数量
    /// </summary>
    public int MaxCount = 21;
    /// <summary>
    /// 背包物品被更改后事件
    /// </summary>
    public event UnityAction OnChange;
    public BagSystem()
    {
        Items = new Dictionary<int, ItemList>();
    }
    public void AddItem(ItemBase item, int addNum = 1)
    {
        if (Items.Count >= MaxCount)
            return;
        foreach (var i in Items)
        {
            if (i.Value.item.name == item.name)
            {
                i.Value.count += 1;
                OnChange();
                return;
            }
        }

        Items.Add(Items.Count, new ItemList(item, addNum));
    }
}
/// <summary>
/// 背包中的物体，包含了物体的数量和对象
/// </summary>
public class ItemList
{
    public ItemList(ItemBase i, int num)
    {
        item = i;
        count = num;
    }
    public ItemBase item;
    public int count;
}
