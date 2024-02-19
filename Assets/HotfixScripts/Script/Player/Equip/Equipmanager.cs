using Fight;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 装备管理器
/// </summary>
public class Equipmanager : MonoBehaviour
{
    public Player player;
    public PlayerInventory inventory;
    public WeaponHolderSlot leftSlot;
    public WeaponHolderSlot rightSlot;
    private CombatEntity PlayerCombatEntity => player.CombatEntity;
    DamageCollider leftCollider;
    DamageCollider rightCollider;
    HandIK handIK;

    Dictionary<EquipType, SkinnedMeshRenderer> EquipRenders;


    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = GetComponent<PlayerInventory>();
        WeaponHolderSlot[] weaponHolderSlots = GetComponentsInChildren<WeaponHolderSlot>();

        foreach (var i in weaponHolderSlots)
        {
            if (i.isLeftHandSlot)
            {
                leftSlot = i;
            }
            else if (i.isRightHandSlot)
            {
                rightSlot = i;
            }
        }
    }
    public void LoadWeaponOnSlot(ItemData item, bool isLeft)
    {
        if (isLeft)
        {
            leftSlot.LoadModel(item);
            LoadLeftWeaponDamageCollider();
        }
        else
        {
            rightSlot.LoadModel(item);
            LoadRightWeaponDamageCollider();
        }
    }
    public void LoadBothWeaponOnSlots()
    {
        LoadWeaponOnSlot(inventory.rightWeapon, false);
        LoadWeaponOnSlot(inventory.leftWeapon, true);
    }
    private void LoadLeftWeaponDamageCollider()
    {
        leftCollider = leftSlot.currentModel.GetComponentInChildren<DamageCollider>();
    }
    private void LoadRightWeaponDamageCollider()
    {
        rightCollider = rightSlot.currentModel.GetComponentInChildren<DamageCollider>();
    }
    public void OpenLeftCollider()
    {
        leftCollider.EnableDamageCollider();
    }
    public void OpenRightCollider()
    {
        rightCollider.EnableDamageCollider();
    }
    public void CloseLeftCollider()
    {
        leftCollider.DisableDamageCollider();
    }
    public void CloseRightCollider()
    {
        rightCollider.DisableDamageCollider();
    }
    public void LoadRightHandIK(bool isTwoHandWeapon)
    {
        handIK = rightSlot.currentModel.GetComponentInChildren<HandIK>();
        //animatorHandle.SetHandIKForWeapon(handIK, isTwoHandWeapon);
    }
    
    public void ReplaceEquip()
    {

    }
}
