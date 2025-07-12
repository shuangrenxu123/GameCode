using Character.Controller.State;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LoginState
{
    public abstract class CharacterLoginBaseState : StateBase<ECharacterLoginState>
    {
        public AnimatorHelper Animancer { get; set; }
        protected CharacterActor characterActor { get; private set; }
        protected CharacterBrain characterBrain { get; private set; }

        protected new CharacterLoginStateMachine parentMachine
          => (CharacterLoginStateMachine)base.parentMachine;

        public CharacterMovementStateMachine movementMachine;

        protected ECharacterMoveState GetMovementState()
        {
            return movementMachine.currentState.currentType;
        }
        
    }
}
