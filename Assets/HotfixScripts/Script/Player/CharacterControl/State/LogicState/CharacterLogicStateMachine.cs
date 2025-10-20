using Character.Controller.State;
using CharacterController;
using HFSM;
using UnityEngine;

namespace Character.Controller.LogicState
{
    public class CharacterLogicStateMachine
        : StateMachine<ECharacterControllerState, ECharacterLogicState>
    {
        public override ECharacterControllerState currentType
            => ECharacterControllerState.Login;

        public CharacterMovementStateMachine movementStateMachine;
        public AnimatorHelper animancer;

        public CharacterLogicStateMachine
            (CharacterActor actor, CharacterBrain characterBrain)
        {
            this.characterBrain = characterBrain;
            this.characterActor = actor;
        }
        public CharacterActor characterActor { get; private set; }
        public CharacterBrain characterBrain { get; private set; }
    }
}
