using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetisInteracting : StateMachineBehaviour
{
    public string target;
    public bool state;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(target,state);
    }
}
