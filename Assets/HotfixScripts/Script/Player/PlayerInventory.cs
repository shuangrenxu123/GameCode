using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色背包管理器
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    private Player player;
    public Equipmanager EquipeManager { get; private set; }
    public ItemData rightWeapon;
    public ItemData leftWeapon;
    private ConsumableItemData currentItem = null;
    private bool CanReplace = true;

    private Dictionary<int, (ItemData, int)> items = new(30);
    public Dictionary<int, (ItemData, int)> Items => items;
    
    public WeaponType WeaponType { get; private set; } = WeaponType.None;
    #region Event 
    public event Action<ItemData, int> OnItemAdd;
    public event Action<ItemData, int> OnItemRemove;
    public event Action<WeaponItemData> OnRightWeaponLoad;
    public event Action<WeaponItemData> OnLeftWeaponLoad;
    #endregion
    private void Awake()
    {
        EquipeManager = GetComponent<Equipmanager>();
        player = GetComponent<Player>();
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
            UseItem(currentItem);   
            //currentItem.AttemptToConsumeItem(animtorHandle, weaponManager);
        }
    }
    /// <summary>
    /// 从背包中使用道具
    /// </summary>
    /// <param name="item"></param>
    public void UseItem(ConsumableItemData item)
    {
        player.SetStateMachineData("interaction", true);
        player.SetStateMachineData("interactionData", item);
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
    public void ReplaceEquipe(EquipType type,Mesh mesh)
    {
        EquipeManager.ReplaceEquip(type, mesh);
    }
    public void ReplaceLeftWeapon(WeaponItemData data)
    {
        EquipeManager.LoadWeaponOnSlot(data,true);
        OnLeftWeaponLoad?.Invoke(data);

    }
    public void ReplaceRightWeapon(WeaponItemData data)
    {
        EquipeManager.LoadWeaponOnSlot(data, false);
        WeaponType = data.WeaponType;
        OnRightWeaponLoad?.Invoke(data);
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
    public void AddItem(ItemData item, int num = 1)
    {
        if (items.ContainsKey(item.id))
        {
            items[item.id] = (items[item.id].Item1, items[item.id].Item2 + num);
        }
        else
        {
            items.Add(item.id, (item, num));
        }
        OnItemAdd?.Invoke(item, num);
        //currentItem = item;
    }

    public void OpenRightDamageCollider()
    {
        if(rightWeapon == null)
        {
            EquipeManager.DefaultRightDamageCollider.EnableDamageCollider();
        }
        else
        {
            EquipeManager.rightCollider.EnableDamageCollider();
        }
    }
    public void OpenLeftDamageCollider()
    {
        if (rightWeapon == null)
        {
            EquipeManager.DefaultLeftDamageCollider.EnableDamageCollider();
        }
        else
        {
            EquipeManager.leftCollider.EnableDamageCollider();
        }
    }

    public void CloseRightDamagerCollider()
    {
        if (rightWeapon == null)
        {
            EquipeManager.DefaultRightDamageCollider.DisableDamageCollider();
        }
        else{
        
               EquipeManager.rightCollider.DisableDamageCollider();
        }
    }
    public void CloseLeftDamagerCollider()
    {
        if (rightWeapon == null)
        {
            EquipeManager.DefaultRightDamageCollider.DisableDamageCollider();
        }
        else
        {

            EquipeManager.rightCollider.DisableDamageCollider();
        }
    }
}
