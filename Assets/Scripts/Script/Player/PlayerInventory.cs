using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    WeaponManager weaponManager;
    public Item rightWeapon;
    public Item leftWeapon;
    private void Awake()
    {
        weaponManager = GetComponentInChildren<WeaponManager>();
    }
    private void Start()
    {
        weaponManager.LoadWeaponOnSlot(rightWeapon,false);
        weaponManager.LoadWeaponOnSlot(leftWeapon,true);
    }
}
