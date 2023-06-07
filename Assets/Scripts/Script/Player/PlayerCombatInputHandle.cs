using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatInputHandle
{
    public Player playerManager;
    AnimatorHandle animatorHandle;
    PlayerInputHandle inputHandle;
    NetTranform NetTranform;
    PlayerInventory inventory;
    public int lastAttackIndex;

    public PlayerCombatInputHandle(Player player)
    {
        playerManager = player;
    }
    public void Init()
    {
        inputHandle = playerManager.inputHandle;
        animatorHandle = playerManager.animatorHandle as AnimatorHandle;
        NetTranform = playerManager.net;
        inventory = playerManager.inventory;
    }

    public void HandleLightAttack()
    {
        animatorHandle.EraseHandIKForWeapon();
        if (playerManager.canDoCombo)
        {
            HandleWeaponCombo(inventory.rightWeapon as WeaponItemData);
        }
        else
        {
            if(playerManager.isInteracting || playerManager.isDefense || playerManager.canDoCombo)
            {
                return;
            }
            HandleLightAttack(inventory.rightWeapon as WeaponItemData);
        }
    }
    public void HandleHeavyAttack()
    {

    }
    public void HandleWeaponCombo(WeaponItemData weapon)
    {
        animatorHandle.anim.SetBool("canDoCombo",false);
        if (inputHandle.TwoHandFlag)
        {
            if(lastAttackIndex != weapon.TH_light_attack_animations.Count -1)
            {
                lastAttackIndex += 1;
                animatorHandle.PlayTargetAnimation(weapon.TH_light_attack_animations[lastAttackIndex],true);
                NetTranform.SendAction(weapon.TH_light_attack_animations[lastAttackIndex]);

            }
        }
        else
        {
            if (lastAttackIndex != weapon.OH_light_attack_Animations.Count - 1)
            {
                lastAttackIndex += 1;
                animatorHandle.PlayTargetAnimation(weapon.OH_light_attack_Animations[lastAttackIndex], true);
                NetTranform.SendAction(weapon.OH_light_attack_Animations[lastAttackIndex]);
                AudioManager.Instance.PlaySound(weapon.light_attack_audios[lastAttackIndex]);
            }
        }
        
    }
    private void HandleLightAttack(WeaponItemData weapon)
    {
        if(inputHandle.sprintFlag)
        {
            animatorHandle.PlayTargetAnimation(weapon.Run_Attack_1, true);
            NetTranform.SendAction(weapon.Run_Attack_1);
        }
        else
        {
            if (inputHandle.TwoHandFlag)
            {
                animatorHandle.PlayTargetAnimation(weapon.TH_light_attack_animations[0],true);
                lastAttackIndex = 0;
                NetTranform.SendAction(weapon.OH_light_attack_Animations[0]);
            }
            else
            {
                animatorHandle.PlayTargetAnimation(weapon.OH_light_attack_Animations[0],true);
                AudioManager.Instance.PlaySound(weapon.light_attack_audios[0]);
                lastAttackIndex = 0;
                NetTranform.SendAction(weapon.OH_light_attack_Animations[0]);
            }
        }
    }
    private void HandleHeavyAttack(WeaponItemData weapon)
    {
        if (inputHandle.TwoHandFlag)
        {

        }
        else
        {
            animatorHandle.PlayTargetAnimation(weapon.OH_Heavy_attack_Animations[0],true);
            NetTranform.SendAction(weapon.OH_Heavy_attack_Animations[0]);
        }
    }
    public void HandleDefense(ArmorItemData armor)
    {
        if (inputHandle.DefenseFlag)
        {
            animatorHandle.anim.SetBool("isDefense", true);
        }
        else
        {
            animatorHandle.anim.SetBool("isDefense", false);
        }
    }
}
