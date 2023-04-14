using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class BTAnimatorAction : BTAction
    {
        private Animator animator;
        private int stateHash;
        private bool loop;
        private bool justEntered = false;

        public BTAnimatorAction(Animator animator,string stateName,bool loop = false,string layerName = "Base Layer")
        {
            this.animator = animator;  
            stateHash = Animator.StringToHash(layerName+"."+stateName);
            this.loop = loop;
        }
        protected override void Enter()
        {
            base.Enter();
            animator.Play(stateHash);
            justEntered = true;

        }
        protected override BTResult Execute()
        {
            if(loop)
            {
                return BTResult.Running;
            }
            
            var info = animator.GetCurrentAnimatorStateInfo(0);
            //当前帧调用了Play方法后，并不能通过GetCurrentAnimatorStateInfo获得我们想要播放的动画，只有下一帧才能正确获得
            //这里设置变量JustEnter来记录是否是第一次进入。如果是就直接返回Running
            if(info.fullPathHash == stateHash || justEntered == true)
            {
                if(justEntered == true)
                {
                    justEntered = false;
                }
                return BTResult.Running;
            }
            return BTResult.Success;
        }
    }
}