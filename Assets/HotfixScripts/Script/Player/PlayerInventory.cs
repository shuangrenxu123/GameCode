using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色背包管理器
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    Equipmanager weaponManager;
    //BagPanel bagPanel;
    private ItemData rightWeapon;
    private ItemData leftWeapon;
    private ConsumableItem currentItem = null;
    private bool CanReplace = true;

    private Dictionary<int, (ItemData, int)> items = new(30);
    public Dictionary<int, (ItemData, int)> Items => items;
    #region Event 
    public event Action<ItemData,int> OnItemAdd;
    public event Action<ItemData,int> OnItemRemove;
    #endregion
    private void Awake()
    {
        weaponManager = GetComponent<Equipmanager>();
    }
    private void Start()
    {
        // weaponManager.LoadWeaponOnSlot(rightWeapon, false);
        // weaponManager.LoadWeaponOnSlot(leftWeapon, true);
        //bagPanel = UIManager.Instance.GetUIWindow<BagPanel>();

        //----------------------Test-------------------------
        //AddItem(new FlaskItem("Flask"));
    }
    public void UserCurrentItem()
    {
        if (currentItem == null)
        {
            return;
        }
        if (CanReplace == true)
        {
            Debug.Log("使用了道具");
            CanReplace = false;
            //currentItem.AttemptToConsumeItem(animtorHandle, weaponManager);
        }
    }
    /// <summary>
    /// 从背包中使用道具
    /// </summary>
    /// <param name="item"></param>
    public void UseItem(ItemData item)
    {
        
    }
    /// <summary>
    /// 替换道具
    /// </summary>
    public void ReplaceItem()
    {

        if (CanReplace == false)
        {
            return;
        }
    }

    public void ReplaceLeftWeapon()
    {

    }
    public void ReplaceRightWeapon()
    {

    }
    /// <summary>
    /// 放置道具
    /// </summary>
    public void PutItem()
    {

    }
    /// <summary>
    /// 拆分道具
    /// </summary>
    public void SplitItme()
    {

    }
    /// <summary>
    /// 添加道具
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(ItemData item,int num = 1)
    {
        //panel.UpdateProp(item);
        if (items.ContainsKey(item.id))
        {
            items[item.id] = (items[item.id].Item1, items[item.id].Item2 + num);
            //bagPanel.AddItem(item, items[item.id].Item2);
        }
        else
        {
            items.Add(item.id, (item, num));
            //ui
            //bagPanel.AddItem(item,num);
        }
        OnItemAdd?.Invoke(item, num);
        //currentItem = item;
    }
}
