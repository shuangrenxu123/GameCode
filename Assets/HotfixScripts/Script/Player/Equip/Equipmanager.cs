using Fight;
using System;
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

    //Dictionary<EquipType, SkinnedMeshRenderer> EquipRenders;
    [SerializeField]
    List<EquipeWarpper> EquipRenders = new List<EquipeWarpper>(8);

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
            //LoadLeftWeaponDamageCollider();
        }
        else
        {
            rightSlot.LoadModel(item);
            //LoadRightWeaponDamageCollider();
        }
    }
    public void LoadBothWeaponOnSlots()
    {
        //LoadWeaponOnSlot(inventory.rightWeapon, false);
        //LoadWeaponOnSlot(inventory.leftWeapon, true);
    }
    /// <summary>
    /// 替换部位装备
    /// </summary>
    /// <param name="qeuipType"></param>
    /// <param name="datas"></param>
    public void ReplaceEquip(EquipType qeuipType, Mesh datas)
    {

        EquipRenders.Find(x => x.EquipType == qeuipType).Mesh.sharedMesh = datas;
    }
    [Serializable]
    class EquipeWarpper
    {
        public EquipType EquipType;
        public SkinnedMeshRenderer Mesh;
    }
}
