using System;
using UnityEngine;

/// <summary>
/// 角色背包管理器
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    Equipmanager weaponManager;
    PlayerInventoryPanel panel;
    public ItemData rightWeapon;
    public ItemData leftWeapon;

    public ConsumableItem currentItem = null;
    public bool CanReplace = true;

    #region Event 
    public event Action<ItemData> OnItemAdd;
    public event Action<ItemData> OnItemRemove;
    #endregion
    private void Awake()
    {
        weaponManager = GetComponent<Equipmanager>();
    }
    private void Start()
    {
        weaponManager.LoadWeaponOnSlot(rightWeapon, false);
        weaponManager.LoadWeaponOnSlot(leftWeapon, true);
        panel = WindowsManager.Instance.GetUiWindow<PlayerInventoryPanel>();

        //----------------------Test-------------------------
        //AddItem(new FlaskItem("Flask"));
    }
    public void UseProps()
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
    /// 替换道具
    /// </summary>
    public void ReplaceItem()
    {
        
        if (CanReplace == false)
        {
            return;
        }
    }
    /// <summary>
    /// 添加道具
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(ConsumableItem item)
    {
        panel.UpdateProp(item);
        currentItem = item;
    }
}
