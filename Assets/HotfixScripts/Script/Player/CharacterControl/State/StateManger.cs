using Animancer;
using Audio;
using Character.Controller.LogicState;
using Character.Controller.MoveState;
using Character.Controller.State;
using CharacterController;
using CharacterController.Camera;
using CharacterControllerStateMachine;
using Fight;
using HFSM;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterControllerStateMachine
{
    public class StateManger : MonoBehaviour
    {

        [SerializeField, ReadOnly]
        ECharacterLogicState loginState;
        [SerializeField, ReadOnly]
        ECharacterMoveState moveState;

        public CharacterMovementStateMachine moveStateMachine;
        public CharacterLogicStateMachine loginMachine;
        public CombatEntity combatEntity;
        public new Camera3D camera;
        public CharacterBrain characterBrain;
        public AnimancerComponent Animancer;
        public AnimatorHelper AnimancerHelper;
        public MaterialControl materialControl;

        [SerializeField]
        private CharacterActor CharacterActor;
        public CCAnimatorConfig animatorConfig;

        [Header("攻击相关")]
        [SerializeField]
        private CharacterWeaponAnimator attackAnimator;
        [SerializeField]
        private WeaponActionChangeGraph actionChangeGraph;

        [SerializeField]
        private SkillRunner skillRunner;
        DataBase<string, object> dataBase;

        private void Awake()
        {
            AnimancerHelper = new AnimatorHelper(Animancer);
            dataBase = new DataBase<string, object>();

            SetStateMachineData("combatEntity", combatEntity);
            InitState();
        }

        void Start()
        {
            moveStateMachine.Start();
            loginMachine.Start();
        }

        private void InitState()
        {
            InitMovementState();
            InitLoginState();
            SetStateParameter();
        }

        void InitMovementState()
        {
            moveStateMachine = new(CharacterActor, characterBrain);

            moveStateMachine.animator = CharacterActor.GetComponentInChildren<Animator>();
            moveStateMachine.database = dataBase;
            moveStateMachine.animancer = AnimancerHelper;

            var lockOnMoveState = new CharacterLockOnMovementState
            {
                movementAnimation = animatorConfig.LockMovement,
            };

            var movementState = new CharacterNormalMovementState
            {
                currentAnimator = animatorConfig.linearMixerAnimators["NormalMove"]
            };

            movementState.lookingDirectionParameters.lookingDirectionMode
                = LookingDirectionParameters.LookingDirectionMode.Movement;

            var crouchMovementState = new CharacterCrouchMovementState
            {
                currentAnimator = animatorConfig.linearMixerAnimators["CrouchMove"]
            };

            var jumpMovement = new CharacterAirMovementState
            {
                jumpAnim = animatorConfig.clipAnimators["Jump"],
                downAnim = animatorConfig.clipAnimators["JumpFall"],
                jumpEndAnim = animatorConfig.clipAnimators["JumpEnd"]
            };

            var climbMovementState = new CharacterClimbState
            {
                Animancer = AnimancerHelper,
                climbAnimations = animatorConfig.climbAnimators
            };

            var runMoveState = new CharacterRunMovementState
            {
                currentAnimator = animatorConfig.linearMixerAnimators["RunMove"]
            };

            moveStateMachine.AddState(movementState);
            moveStateMachine.AddState(crouchMovementState);
            moveStateMachine.AddState(jumpMovement);
            moveStateMachine.AddState(climbMovementState);
            moveStateMachine.AddState(lockOnMoveState);
            moveStateMachine.AddState(runMoveState);

        }
        void InitLoginState()
        {
            loginMachine = new(CharacterActor, characterBrain);
            loginMachine.movementStateMachine = moveStateMachine;
            loginMachine.animancer = AnimancerHelper;
            loginMachine.database = dataBase;
            var emptyState = new CharacterEmptyLogicState();
            var interactState = new CharacterInteractionState()
            {
                interactAnimations = animatorConfig.clipAnimators
            };

            var attackState = new CharacterAttackState()
            {
                attackAnimator = attackAnimator,
                actionChangeGraph = actionChangeGraph,
                timelineExecutor = skillRunner,
            };
            loginMachine.AddState(attackState);
            loginMachine.AddState(emptyState);
            loginMachine.AddState(interactState);
            loginMachine.SetDefaultState(ECharacterLogicState.Empty);
        }

        void SetStateParameter()
        {
            if (camera)
                moveStateMachine.ExternalReference = camera.transform;
        }

        private void Update()
        {
            moveStateMachine.Update();
            loginMachine.Update();
            loginState = loginMachine.CurrentStateType;
            moveState = moveStateMachine.CurrentStateType;
        }

        private void FixedUpdate()
        {
            moveStateMachine.FixUpdate();
            loginMachine.FixUpdate();
        }

        public void SetStateMachineData(string key, object value)
        {
            dataBase.SetData(key, value);
        }
    }
}