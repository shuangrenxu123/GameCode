using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class AnimatorHandle : AnimatorManager
{
    public Player playerManager;
    public PlayerInputHandle inputHandle;
    public PlayerController controller;
    public PlayerInventory inventory;
    public PlayerCombatEntity entity;
    public Equipmanager equipmanager;

    int vertical;
    int horizontal;

    bool handIKWeightReset = false;

    protected RigBuilder rigBuilder; 
    public TwoBoneIKConstraint leftHandConstraint;
    public TwoBoneIKConstraint rightHandConstraint;

    public void Initialize()
    {
        playerManager = GetComponent<Player>();
        vertical = Animator.StringToHash("Verical");
        horizontal = Animator.StringToHash("Horizontal");
        inputHandle = GetComponent<PlayerInputHandle>();
        entity = GetComponent<PlayerCombatEntity>();
        inventory = GetComponent<PlayerInventory>();
        equipmanager = GetComponent<Equipmanager>();
        controller = playerManager.controller;
        rigBuilder = GetComponent<RigBuilder>();    

    }
    public void UpdateAnimatorValues(float verticalMovement,float horizontalMovement,bool isSprinting)
    {
        #region Vertical
        float v = 0;
        if(verticalMovement > 0 && verticalMovement<0.55)
        {
            v = 0.5f;
        }
        else if(verticalMovement > 0.55)
        {
            v = 1f;
        }
        else if(verticalMovement <0&& verticalMovement>-0.55)
        {
            v = -0.5f;
        }
        else if(verticalMovement < -0.55)
        {
            v = -1;
        }
        else
        {
            v = 0;
        }
        #endregion
        #region horizontal
        float h = 0;
        if(horizontalMovement>0 && horizontalMovement < 0.55)
        {
            h = 0.5f;
        }
        else if (horizontalMovement > 0.55f)
        {
            h = 1f;
        }
        else if(horizontalMovement <0 && horizontalMovement > -0.55)
        {
            h = -0.5f;
        }
        else if(horizontalMovement < -0.55)
        {
            h = -1;
        }
        else
        {
            h = 0;
        }
        #endregion
        if(isSprinting)
        {
            v = 2;
            h = horizontalMovement;
        }
        anim.SetFloat(vertical,v, 0.1f,Time.deltaTime);
        anim.SetFloat(horizontal,h,0.1f,Time.deltaTime);
    }
    private void OnAnimatorMove()
    {
        if(playerManager.isInteracting == false)
        {
            return;
        }
        Vector3 velocity = playerManager.animatorHandle.anim.deltaPosition;
        playerManager.characterController.Move(velocity);
        playerManager.transform.rotation *= playerManager.animatorHandle.anim.deltaRotation;
    }
    public void CanRotate()
    {
        anim.SetBool("canRotate",true);
    }
    public void StopRotate()
    {
        anim.SetBool("canRotate", false);
    }
    public void EnableAttackCombo()
    {
        anim.SetBool("canDoCombo",true);
    }
    public void DisableAttackCombo()
    {
        anim.SetBool("canDoCombo", false);
    }
    public void PropsCallBack()
    {
        inventory.currentItem.Effect(entity,equipmanager);
        inventory.CanrReplace = true;
    }

    public void SetHandIKForWeapon(HandIK target,bool isTwoHandlingWeapon)
    {
        if(isTwoHandlingWeapon)
        {
            //缓存一份右手值
            Vector3 rightpos = target.rightTransform.position;
            var rotation = target.rightTransform.rotation;
            var weapon = equipmanager.rightSlot.currentModel;
            var hand = weapon.transform.parent;
            //获得手的父物体
            weapon.transform.SetParent(null);
            hand.transform.position = rightpos;
            hand.transform.rotation = rotation;
            weapon.transform.SetParent(hand);

            leftHandConstraint.data.target = target.leftTransform;
            leftHandConstraint.data.targetRotationWeight = 1;
            leftHandConstraint.data.targetPositionWeight = 1;
        }
        else
        {
            rightHandConstraint.data.target = null;
            leftHandConstraint.data.target  = null;
        }
        rigBuilder.Build();
    }
    public void EraseHandIKForWeapon()
    {
        handIKWeightReset = true;
        if(leftHandConstraint.data.target != null)
        {
            leftHandConstraint.data.targetPositionWeight = 0;
            leftHandConstraint.data.targetRotationWeight = 0;
        }
    }
}
