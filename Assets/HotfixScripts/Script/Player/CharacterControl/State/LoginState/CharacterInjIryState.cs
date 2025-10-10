using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using HFSM;
using UnityEngine;
namespace Character.Controller.LoginState
{
    public record CharacterInjIryStateInput(int hitAngle) : StateBaseInput;

    public class CharacterInjIryState : CharacterLogicBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.InjIry;

        public Dictionary<string, ClipTransition> injIryAnimations;

        ClipTransition targetAnimation;

        public override void Enter(StateBaseInput input)
        {
            base.Enter();

            characterActor.Velocity = Vector3.zero;
            var injIryInput = input as CharacterInjIryStateInput;


            //Test
            var state = Animancer.Play(injIryAnimations["0"]);

            state.Events.OnEnd = () =>
            {
                ChangeNextState();
                state.Events.OnEnd = null;
            };
            parentMachine.movementStateMachine.EnableMachine(false, false);
        }

        void ChangeNextState()
        {
            if (combatEntity.hp.Value <= 0)
            {
                parentMachine.ChangeState(ECharacterLoginState.Death);
            }
            else
            {
                parentMachine.ChangeState(ECharacterLoginState.Empty);
            }
        }
        public override void Exit()
        {
            base.Exit();
            parentMachine.movementStateMachine.EnableMachine(true, true);
            parentMachine.movementStateMachine.RefreshAnimator();
        }
    }
}
