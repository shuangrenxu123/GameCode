using System.Collections.Generic;
using UnityEngine.Events;

public class BagSystem
{
    public Dictionary<int, ItemList> Items;
    /// <summary>
    /// �����������
    /// </summary>
    public int MaxCount = 21;
    /// <summary>
    /// ������Ʒ�����ĺ��¼�
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
/// �����е����壬����������������Ͷ���
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
