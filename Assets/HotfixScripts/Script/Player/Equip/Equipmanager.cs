using Fight;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 装备管理器
/// </summary>
public class Equipmanager : MonoBehaviour
{
    private Player player;
    private PlayerInventory inventory;
    public WeaponHolderSlot leftSlot;
    public WeaponHolderSlot rightSlot;
    private CombatEntity PlayerCombatEntity => player.CombatEntity;
    public DamageCollider leftCollider;
    public DamageCollider rightCollider;

    public DamageCollider DefaultRightDamageCollider;
    public DamageCollider DefaultLeftDamageCollider;
    //HandIK handIK;

    //Dictionary<EquipType, SkinnedMeshRenderer> EquipRenders;
    [SerializeField]
    List<EquipeWarpper> EquipRenders = new List<EquipeWarpper>(8);
    [SerializeField]
    List<HeadWarpper> headWarppers = new List<HeadWarpper>(3);
    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = GetComponent<PlayerInventory>();
    }
    public void LoadWeaponOnSlot(WeaponItemData item, bool isLeft)
    {
        if (isLeft)
        {
            leftCollider = leftSlot.LoadWeapon(item).GetComponentInChildren<DamageCollider>();
            //LoadLeftWeaponDamageCollider();
        }
        else
        {
            rightCollider = rightSlot.LoadWeapon(item).GetComponentInChildren<DamageCollider>();
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
        if (qeuipType == EquipType.helmet)
        {
            foreach (var h in headWarppers)
            {
                h.Mesh.gameObject.SetActive(false);
            }
            EquipRenders.Find(x => x.EquipType == qeuipType).Mesh.gameObject.SetActive(true);
        }

        EquipRenders.Find(x => x.EquipType == qeuipType).Mesh.sharedMesh = datas;


    }
    [Serializable]
    class EquipeWarpper
    {
        public EquipType EquipType;
        public SkinnedMeshRenderer Mesh;
    }
    [Serializable]
    class HeadWarpper
    {
        public Decoration type;
        public SkinnedMeshRenderer Mesh;
    }
}
