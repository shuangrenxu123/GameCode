using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetAnimator : MonoBehaviour
{
    public Animator anim;
    int vertical;
    int horizontal;
    private void Start()
    {
        vertical = Animator.StringToHash("Verical");
        horizontal = Animator.StringToHash("Horizontal");
    }
    public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting)
    {
        #region Vertical
        float v = 0;
        if (verticalMovement > 0 && verticalMovement < 0.55)
        {
            v = 0.5f;
        }
        else if (verticalMovement > 0.55)
        {
            v = 1f;
        }
        else if (verticalMovement < 0 && verticalMovement > -0.55)
        {
            v = -0.5f;
        }
        else if (verticalMovement < -0.55)
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
        if (horizontalMovement > 0 && horizontalMovement < 0.55)
        {
            h = 0.5f;
        }
        else if (horizontalMovement > 0.55f)
        {
            h = 1f;
        }
        else if (horizontalMovement < 0 && horizontalMovement > -0.55)
        {
            h = -0.5f;
        }
        else if (horizontalMovement < -0.55)
        {
            h = -1;
        }
        else
        {
            h = 0;
        }
        #endregion
        if (isSprinting)
        {
            v = 2;
            h = horizontalMovement;
        }
        anim.SetFloat(vertical, v, 0.1f, Time.deltaTime);
        anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
    }
    public void PlayTargetAnimation(string targetAnim, bool isInteracting)
    {
        //anim.applyRootMotion = isInteracting;
        anim.SetBool("isInteracting", isInteracting);
        anim.CrossFade(targetAnim, 0.1f);
    }
}
