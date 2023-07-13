using UnityEngine;

public class Equipmanager : MonoBehaviour
{
    public Player player;
    public PlayerInventory inventory;
    public WeaponHolderSlot leftSlot;
    public WeaponHolderSlot rightSlot;
    public PlayerInputHandle inputHandle;
    public AnimatorHandle animatorHandle;
    DamageCollider leftCollider;
    DamageCollider rightCollider;
    HandIK handIK;
    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = GetComponent<PlayerInventory>();
        inputHandle = GetComponent<PlayerInputHandle>();
        animatorHandle = GetComponent<AnimatorHandle>();
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
    private void Start()
    {

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
            animatorHandle.anim.CrossFade("Both Empty", 0.2f);

            if (inputHandle.TwoHandFlag)
            {
                animatorHandle.anim.CrossFade(((WeaponItemData)item).TH_Idle, 0.2f);
            }
            rightSlot.LoadModel(item);
            LoadRightWeaponDamageCollider();
            LoadRightHandIK(inputHandle.TwoHandFlag);
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
        animatorHandle.SetHandIKForWeapon(handIK, isTwoHandWeapon);
    }
}
