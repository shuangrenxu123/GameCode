using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using HFSM;
using UnityEngine;
namespace Character.Controller.LogicState
{
    public record CharacterInjIryStateInput(int hitAngle) : StateBaseInput;

    public class CharacterInjIryState : CharacterLogicBaseState
    {
        public override ECharacterLogicState currentType => ECharacterLogicState.InjIry;

        public Dictionary<string, ClipTransition> injIryAnimations;

        AnimancerState injuryAnimationState;

        public override void Enter(StateBaseInput input)
        {
            base.Enter();

            characterActor.Velocity = Vector3.zero;
            //Test
            injuryAnimationState = Animancer.Play(injIryAnimations["0"]);

            injuryAnimationState.Events.OnEnd = () =>
            {
                injuryAnimationState.Events.OnEnd = null;
                injuryAnimationState = null;
                ChangeNextState();
            };
            parentMachine.movementStateMachine.EnableMachine(false, false);
        }

        void ChangeNextState()
        {
            if (combatEntity.hp.Value <= 0)
            {
                parentMachine.ChangeState(ECharacterLogicState.Death);
            }
            else
            {
                parentMachine.ChangeState(ECharacterLogicState.Empty);
            }
        }
        public override void Exit()
        {
            base.Exit();

            if (injuryAnimationState != null)
            {
                injuryAnimationState.Events.OnEnd = null;
                injuryAnimationState = null;
            }

            parentMachine.movementStateMachine.EnableMachine(true, true);
            parentMachine.movementStateMachine.RefreshAnimator();

        }
    }
}
