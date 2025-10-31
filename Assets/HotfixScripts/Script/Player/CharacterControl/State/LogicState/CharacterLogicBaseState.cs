using Character.Controller.State;
using CharacterController;
using Fight;
using GameLogin.Interact;
using HFSM;
using UnityEngine;
namespace Character.Controller.LogicState
{
    public abstract class CharacterLogicBaseState : StateBase<ECharacterLogicState>
    {
        public AnimatorHelper Animancer { get; set; }
        protected CharacterActor characterActor { get; private set; }
        protected CharacterBrain characterBrain { get; private set; }

        protected CombatEntity combatEntity;
        protected new CharacterLogicStateMachine parentMachine
          => (CharacterLogicStateMachine)base.parentMachine;

        public override void Init()
        {
            base.Init();
            characterActor = parentMachine.characterActor;
            characterBrain = parentMachine.characterBrain;
            Animancer = parentMachine.animancer;
            combatEntity = parentMachine.database.GetData<CombatEntity>("combatEntity");
        }
        public CharacterActions characterActions
        {
            get
            {
                return characterBrain == null ?
                    new CharacterActions() : characterBrain.CharacterActions;
            }
        }
        public CharacterMovementStateMachine movementMachine => parentMachine.movementStateMachine;

        protected ECharacterMoveState GetMovementState()
        {
            return movementMachine.currentState.currentType;
        }

    }
}
