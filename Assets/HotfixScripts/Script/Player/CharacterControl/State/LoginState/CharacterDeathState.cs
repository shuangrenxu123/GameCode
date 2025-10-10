using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using HFSM;
using UnityEngine;

namespace Character.Controller.LoginState
{
    public class CharacterDeathState : CharacterLogicBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.Death;

        public Dictionary<string, ClipTransition> deathAnimations;
        public Dictionary<string, ClipTransition> deathLoopAnimations;

        ClipTransition deathLoopAnimation;
        public override void Enter(StateBaseInput input = null)
        {
            base.Enter(input);
            characterActor.Velocity = Vector3.zero;

            parentMachine.movementStateMachine.EnableMachine(false, false);

            characterActor.ColliderComponent.Collider.enabled = false;

            deathLoopAnimation = deathLoopAnimations["0"];
            var state = Animancer.Play(deathAnimations["0"]);
            state.Events.OnEnd = () =>
            {
                Animancer.Play(deathLoopAnimation);
                state.Events.OnEnd = null;
            };
        }

        public override void Exit()
        {
            base.Exit();
            characterActor.ColliderComponent.Collider.enabled = true;
            parentMachine.movementStateMachine.EnableMachine(true, true);
        }
    }
}