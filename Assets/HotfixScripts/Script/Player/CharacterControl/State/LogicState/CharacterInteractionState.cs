using System;
using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LogicState
{
    public record InteractionState(string animClipName,
        bool needSetPosition,
        bool useRootMotion,
        Vector3 position,
        Quaternion rotation,
        Action onEnter,
        Action onExit) : StateBaseInput;

    public class CharacterInteractionState : CharacterLogicBaseState
    {
        public override ECharacterLogicState currentType => ECharacterLogicState.Interaction;
        public Dictionary<string, ClipTransition> interactAnimations;
        InteractionState inputData;
        AnimancerState interactionAnimationState;

        public override void Enter(StateBaseInput input = null)
        {
            base.Enter();

            inputData = input as InteractionState;
            // //确定坐标
            characterActor.Velocity = Vector3.zero;

            if (inputData.needSetPosition)
            {
                characterActor.Teleport(inputData.position, inputData.rotation);
            }
            parentMachine.movementStateMachine.EnableMachine(false, false);

            //RootMotion
            if (inputData.useRootMotion)
            {
                characterActor.SetUpRootMotion(true, true);
            }
            interactionAnimationState = Animancer.Play(interactAnimations[inputData.animClipName]);
            interactionAnimationState.Events.OnEnd = () =>
            {
                interactionAnimationState.Events.OnEnd = null;
                interactionAnimationState = null;
                parentMachine.ChangeState(ECharacterLogicState.Empty);
            };

            inputData.onEnter?.Invoke();
        }

        public override void Exit()
        {
            base.Exit();

            if (interactionAnimationState != null)
            {
                interactionAnimationState.Events.OnEnd = null;
                interactionAnimationState = null;
            }

            inputData.onExit?.Invoke();

            parentMachine.movementStateMachine.EnableMachine(true, true);
            parentMachine.movementStateMachine.RefreshAnimator();
            characterActor.UseRootMotion = false;
        }

    }
}
