using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    AnimatorHandle animatorHandle;
    PlayerInputHandle inputHandle;
    public string lastAttack;
    private void Awake()
    {
        inputHandle = GetComponent<PlayerInputHandle>();
        animatorHandle = GetComponentInChildren<AnimatorHandle>();
    }
    public void HandleWeaponCombo(WeaponItem weapon)
    {
        if (inputHandle.comboFlag)
        {

            animatorHandle.anim.SetBool("canDoCombo", false);
            if (lastAttack == weapon.Light_Attack_1)
            {
                animatorHandle.PlayTargetAnimation(weapon.Light_Attack_2, true);
            }
        }
    }
    public void HandleLightAttack(WeaponItem weapon)
    {
        animatorHandle.PlayTargetAnimation(weapon.Light_Attack_1,true);
        lastAttack = weapon.Light_Attack_1;
    }
    public void HandleHeavyAttack(WeaponItem weapon)
    {
        animatorHandle.PlayTargetAnimation(weapon.Heavy_Attack_1,true);
    }
    public void HandleDefense(ArmorItem armor)
    {
        if (inputHandle.DefenseFlag)
        {
            animatorHandle.anim.SetBool("isDefense", true);
        }
        else
        {
            animatorHandle.anim.SetBool("isDefense", false);

        }
        //animatorHandle.PlayTargetAnimation(armor.armorAnimator, false);
    }
}
