using Animancer;
using System;
using UnityEngine;
namespace BT.Nodes
{
    public class BTAnimation : BTAction
    {
        AnimancerComponent animancer;
        CCAnimatorConfig config;
        bool isEnd = false;
        public BTAnimation(AnimancerComponent anim,CCAnimatorConfig config, string name) : base(name)
        {
            this.animancer = anim;
            this.config = config;   
        }
        protected override void Enter()
        {
            base.Enter();
            var state = animancer.Play(config.clipAnimators[name]);
            state.Events.OnEnd += OnAnimationEnd;
        }
        protected override BTResult Execute()
        {
            return isEnd == true ? BTResult.Success : BTResult.Running;
        }
        protected override void Exit()
        {
            isEnd =false;
            base.Exit();
        }
        private void OnAnimationEnd()
        {
            isEnd = true;
        }
    }
}
