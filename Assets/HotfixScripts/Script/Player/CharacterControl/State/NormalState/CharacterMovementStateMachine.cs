using Character.Controller.MoveState;
using HFSM;
using UnityEngine;
namespace Character.Controller.State
{
    public class CharacterMovementStateMachine
        : StateMachine<ECharacterControllerState, ECharacterMoveState>
    {

        public bool enable { get; private set; } = true;
        #region parameters
        public MovementReferenceParameters movementReferenceParameters = new();
        public Vector3 InputMovementReference => movementReferenceParameters.InputMovementReference;
        #endregion

        public override ECharacterControllerState currentType => ECharacterControllerState.Move;

        public Transform ExternalReference
        {
            get => movementReferenceParameters.externalReference;
            set => movementReferenceParameters.externalReference = value;
        }

        public MovementReferenceParameters.MovementReferenceMode MovementReferenceMode
        {
            get => movementReferenceParameters.movementReferenceMode;
            set => movementReferenceParameters.movementReferenceMode = value;
        }
        public Vector3 MovementReferenceForward => movementReferenceParameters.MovementReferenceForward;
        public Vector3 MovementReferenceRight => movementReferenceParameters.MovementReferenceRight;
        public CharacterActor characterActor { get; private set; }
        public CharacterBrain characterBrain { get; private set; }
        public Animator animator { get; set; }
        public AnimatorHelper animancer;
        public MaterialControl materialControl;
        public CharacterMovementStateMachine(CharacterActor characterActor, CharacterBrain brain)
        {
            this.characterActor = characterActor;
            this.characterBrain = brain;
        }

        public new CharacterMovementStateBase currentState
            => (CharacterMovementStateBase)base.currentState;

        void PreCharacterSimulation(float dt)
        {
            if (!enable)
            {
                return;
            }
            currentState.PreCharacterSimulation();
        }
        void PostCharacterSimulation(float dt)
        {
            if (!enable)
            {
                return;
            }
            currentState.PostCharacterSimulation();
        }

        public override void Init()
        {
            characterActor.OnPreSimulation += PreCharacterSimulation;
            characterActor.OnPostSimulation += PostCharacterSimulation;
            if (animator != null)
            {
                characterActor.OnAnimatorIKEvent += OnAnimatorIK;
            }
            movementReferenceParameters.Initialize(characterActor);
            base.Init();
        }
        public override void Exit()
        {
            characterActor.OnPreSimulation -= PreCharacterSimulation;
            characterActor.OnPostSimulation -= PostCharacterSimulation;
            if (animator != null)
                characterActor.OnAnimatorIKEvent -= OnAnimatorIK;
        }
        public override void Update()
        {
            if (!enable)
            {
                return;
            }
            base.Update();
        }

        public override void FixUpdate()
        {
            if (!enable)
            {
                return;
            }
            movementReferenceParameters.UpdateData(characterBrain.CharacterActions.movement.value);
            currentState.FixUpdate();
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (currentState == null)
                return;

            currentState.UpdateIK(layerIndex);
        }

        public void ResetIKWeights()
        {
            characterActor.ResetIKWeights();
        }

        public void DisableMachine()
        {
            enable = false;
        }
        public void EnableMachine()
        {
            enable = true;
            animancer.Play(currentState.currentAnimator);
        }
    }
}
