using Character.Controller.State;
using CharacterController;
using HFSM;
using UnityEngine;

namespace Character.Controller.LoginState
{
    public class CharacterLoginStateMachine
        : StateMachine<ECharacterControllerState, ECharacterLoginState>
    {
        public override ECharacterControllerState currentType
            => ECharacterControllerState.Login;

        public CharacterMovementStateMachine movementStateMachine;
        public AnimatorHelper animancer;

        public CharacterLoginStateMachine
            (CharacterActor actor, CharacterBrain characterBrain)
        {
            this.characterBrain = characterBrain;
            this.characterActor = actor;
        }
        public CharacterActor characterActor { get; private set; }
        public CharacterBrain characterBrain { get; private set; }
    }
}
