using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHandle : MonoBehaviour
{
    public Player playerManager;
    public Animator anim;
    public PlayerInputHandle inputHandle;
    public PlayerController controller;
    int vertical;
    int horizontal;
    public bool canRotate;

    public void Initialize()
    {
        playerManager = GetComponentInParent<Player>();
        anim =  GetComponent<Animator>();
        vertical = Animator.StringToHash("Verical");
        horizontal = Animator.StringToHash("Horizontal");
        inputHandle = GetComponentInParent<PlayerInputHandle>();
        controller = GetComponentInParent<PlayerController>();

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
        anim.SetFloat(vertical,v,0.1f,Time.deltaTime);
        anim.SetFloat(horizontal,h,0.1f,Time.deltaTime);
    }
    public void PlayTargetAnimation(string targetAnim,bool isInteracting)
    {
        anim.applyRootMotion = isInteracting;
        anim.SetBool("isInteracting", isInteracting);
        anim.CrossFade(targetAnim, 0.1f); 
    }
    private void OnAnimatorMove()
    {
        if(playerManager.isInteracting == false)
        {
            return;
        }
        float delta = Time.deltaTime;
        controller.rb.drag = 0;
        Vector3 deltaPosition = anim.deltaPosition;
        deltaPosition.y = 0;
        Vector3 velocity = deltaPosition / delta;
        controller.rb.velocity = velocity;
    }
    public void CanRotate()
    {
        canRotate = true;
    }
    public void StopRotate()
    {
        canRotate = false;
    }

    public void EnableAttackCombo()
    {
        anim.SetBool("canDoCombo", true);
    }
    public void DisableAttackCombo()
    {
        anim.SetBool("canDoCombo", false);
    }
}
