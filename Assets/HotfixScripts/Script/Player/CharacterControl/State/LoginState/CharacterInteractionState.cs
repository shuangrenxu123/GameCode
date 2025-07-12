using System.Collections.Generic;
using Animancer;
using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LoginState
{
    public class CharacterInteractionState : CharacterLoginBaseState
    {
        public override ECharacterLoginState currentType => ECharacterLoginState.Interaction;
        public Dictionary<string, ClipTransition> interactAnimations;
        public override void Enter()
        {
            base.Enter();
            var trigger = characterActor.Triggers[0];
            var interaction = trigger.gameObject.GetComponent<Intractable>();

            characterActor.Position = interaction.reference.position;
            characterActor.SetYaw(interaction.reference.forward);

            Animancer.Play(interactAnimations[interaction.intractableType.ToString()]);
            interaction.Interactive();
        }
    }
}
