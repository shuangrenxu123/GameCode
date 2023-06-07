using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    AnimatorHandle animtorHandle;
    Equipmanager weaponManager;
    PlayerInventoryPanel panel;
    public ItemData rightWeapon;
    public ItemData leftWeapon;

    public ConsumableItem currentItem = null;
    public bool CanrReplace = true;
    private void Awake()
    {
        weaponManager = GetComponent<Equipmanager>();
        animtorHandle = GetComponent<AnimatorHandle>();
    }
    private void Start()
    {
        weaponManager.LoadWeaponOnSlot(rightWeapon,false);
        weaponManager.LoadWeaponOnSlot(leftWeapon,true);
        panel = WindowsManager.Instance.GetUiWindow<PlayerInventoryPanel>();

        //----------------------Test-------------------------
        AddItem(new FlaskItem("Flask"));
    }
    public void UseProps()
    {
        if (CanrReplace == true)
        {
            Debug.Log("使用了道具");
            CanrReplace = false;
            currentItem.AttemptToConsumeItem(animtorHandle,weaponManager);
        }
    }
    public void ReplaceItem()
    {
        if(CanrReplace ==  false)
        {
            return;
        }
    }

    public void AddItem(ConsumableItem item)
    {
        panel.UpdateProp(item);
        currentItem = item;
    }
}
