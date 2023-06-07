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
    public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement)
    {
        float v = 0;
        if (verticalMovement > 0.1 && verticalMovement < 0.55)
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
        else if(verticalMovement > 1f)
        {
            v = 2;
        }
        else
        {
            v = 0;
        }
        float h = 0;
        if (horizontalMovement > 0.1 && horizontalMovement < 0.55)
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
        anim.SetFloat(vertical,v,0.1f,Time.deltaTime);
        anim.SetFloat(horizontal,h, 0.1f, Time.deltaTime);
    }
    public void PlayTargetAnimation(string targetAnim)
    {
        Debug.Log(targetAnim); 
        anim.CrossFade(targetAnim, 0.1f);
    }
}
