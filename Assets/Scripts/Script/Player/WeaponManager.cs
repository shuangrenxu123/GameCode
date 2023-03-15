using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponHolderSlot leftSlot;
    public WeaponHolderSlot rightSlot;

    DamageCollider leftCollider;
    DamageCollider rightCollider;

    private void Awake()
    {
        WeaponHolderSlot[] weaponHolderSlots= GetComponentsInChildren<WeaponHolderSlot>();
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
    public void LoadWeaponOnSlot(Item item,bool isLeft)
    {
        if (isLeft)
        {
            leftSlot.LoadWeaponModel(item);
            LoadLeftWeaponDamageCollider();
        }
        else
        {
            rightSlot.LoadWeaponModel(item);
            LoadRightWeaponDamageCollider();
        }
    }

    private void LoadLeftWeaponDamageCollider()
    {
        leftCollider = leftSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
    }
    private void LoadRightWeaponDamageCollider()
    {
        rightCollider =rightSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
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
}
