using AIBlackboard;
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
using static CharacterController.MovementReferenceParameters;

namespace Character.Controller
{
    public class NPCStateMgr : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        ECharacterLogicState loginState;
        [SerializeField, ReadOnly]
        ECharacterMoveState moveState;

        [SerializeField]
        CombatEntity combatEntity;

        [SerializeField]
        CharacterBrain characterBrain;

        [SerializeField]
        AnimancerComponent Animancer;

        AnimatorHelper AnimancerHelper;

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

        public CharacterMovementStateMachine moveStateMachine;
        public CharacterLogicStateMachine loginMachine;
        Blackboard dataBase;

        private void Awake()
        {
            AnimancerHelper = new AnimatorHelper(Animancer);
            dataBase = new Blackboard();
        }
        private void Start()
        {
            SetStateMachineData("combatEntity", combatEntity);

            InitState();
            moveStateMachine.movementReferenceParameters.movementReferenceMode = MovementReferenceMode.World;
            moveStateMachine.Start();
            loginMachine.Start();
        }

        private void InitState()
        {
            InitMovementState();
            InitLoginState();
            // controller.SetDefaultState(ECharacterMoveState.Climb);
        }

        void InitMovementState()
        {
            moveStateMachine = new(CharacterActor, characterBrain);

            moveStateMachine.animator = CharacterActor.GetComponentInChildren<Animator>();
            moveStateMachine.database = dataBase;
            moveStateMachine.animancer = AnimancerHelper;

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


            moveStateMachine.AddState(movementState);
            moveStateMachine.AddState(crouchMovementState);
            moveStateMachine.AddState(jumpMovement);
            moveStateMachine.AddState(climbMovementState);

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
            var CharacterInjIryState = new CharacterInjIryState()
            {
                injIryAnimations = animatorConfig.injIryAnimators,
            };

            var deathState = new CharacterDeathState()
            {
                deathAnimations = animatorConfig.DeathAnimators,
                deathLoopAnimations = animatorConfig.DeathLoopAnimators
            };

            loginMachine.AddState(deathState);
            loginMachine.AddState(attackState);
            loginMachine.AddState(emptyState);
            loginMachine.AddState(interactState);
            loginMachine.AddState(CharacterInjIryState);

            loginMachine.SetDefaultState(ECharacterLogicState.Empty);
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
            dataBase.SetValue(key, value);
        }
    }
}