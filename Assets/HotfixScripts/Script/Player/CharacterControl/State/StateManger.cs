using Animancer;
using Audio;
using Character.Controller.LoginState;
using Character.Controller.MoveState;
using Character.Controller.State;
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
        ECharacterLoginState loginState;
        [SerializeField, ReadOnly]
        ECharacterMoveState moveState;

        public CharacterMovementStateMachine moveStateMachine;
        public CharacterLoginStateMachine loginMachine;
        public Player player;
        public new Camera3D camera;
        public CharacterBrain characterBrain;
        public AnimancerComponent Animancer;
        public AnimatorHelper AnimancerHelper;
        public MaterialControl materialControl;
        public AudioData moveData;

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
        }
        private void Start()
        {
            player = GetComponentInParent<Player>();

            InitState();

            SetStateMachineData("CombatEntity", player.CombatEntity);


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
            moveStateMachine.ExternalReference = camera.transform;
            moveStateMachine.animator = CharacterActor.GetComponentInChildren<Animator>();
            moveStateMachine.database = dataBase;
            moveStateMachine.animancer = AnimancerHelper;
            moveStateMachine.stateManger = this;

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
                database = dataBase,
                Animancer = AnimancerHelper,
                climbAnimations = animatorConfig.climbAnimators
            };

            moveStateMachine.AddState(movementState);
            moveStateMachine.AddState(crouchMovementState);
            moveStateMachine.AddState(jumpMovement);
            moveStateMachine.AddState(climbMovementState);
            moveStateMachine.AddState(lockOnMoveState);


        }
        void InitLoginState()
        {
            loginMachine = new(CharacterActor, characterBrain);
            loginMachine.movementStateMachine = moveStateMachine;
            loginMachine.animancer = AnimancerHelper;
            var emptyState = new CharacterEmptyLoginState();
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
            loginMachine.SetDefaultState(ECharacterLoginState.Empty);
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

        public void HandleLock()
        {
            // var movestate = controller.FindState("move") as MovementState;
            // movestate?.HandleLockEnemy(camera.currentLockOnTarget);
        }

        public void SetStateMachineData(string key, object value)
        {
            moveStateMachine.database.SetData(key, value);
        }
    }
}