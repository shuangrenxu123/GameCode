using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LoginState
{
    public class CharacterInteractionState : CharacterLogicBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.Interaction;
        public Dictionary<string, ClipTransition> interactAnimations;
        public override void Enter()
        {
            base.Enter();
            var trigger = characterActor.Triggers[0];
            var interaction = trigger.gameObject.GetComponent<Intractable>();

            // //确定坐标
            // characterActor.Position = interaction.reference.position;
            // characterActor.SetYaw(interaction.reference.forward);
            characterActor.Velocity = Vector3.zero;
            parentMachine.movementStateMachine.EnableMachine(false, false);
            characterActor.Teleport(interaction.reference.position, interaction.reference.rotation);

            //RootMotion
            if (interaction.UseRootMotion)
            {
                characterActor.SetUpRootMotion(true, true);
            }
            var animState = Animancer.Play(interactAnimations[interaction.intractableType.ToString()]);
            animState.Events.OnEnd = () =>
            {
                parentMachine.ChangeState(ECharacterLoginState.Empty);
            };


            interaction.Interactive();
        }

        public override void Exit()
        {
            base.Exit();
            parentMachine.movementStateMachine.EnableMachine(true, true);
            characterActor.UseRootMotion = false;
        }

    }
}
