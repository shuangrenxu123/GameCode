using Animancer;
using Audio;
using Character.Controller.LoginState;
using Character.Controller.MoveState;
using Character.Controller.State;
using CharacterControllerStateMachine;
using HFSM;
using UnityEngine;

namespace CharacterControllerStateMachine
{
    public class StateManger : MonoBehaviour
    {
        public CharacterMovementStateMachine stateMachine;
        public CharacterLoginStateMachine loginMachine;
        public Player player;
        public new Camera3D camera;
        private CharacterActor CharacterActor;
        public CharacterBrain characterBrain;
        public AnimancerComponent Animancer;
        public AnimatorHelper AnimancerHelper;
        public MaterialControl materialControl;
        public AudioData moveData;

        [SerializeField]
        public CCAnimatorConfig animatorConfig;
        [Header("��������")]
        [SerializeField]
        private CharacterWeaponAnimator attackAnimator;

        DataBase dataBase;

        private void Awake()
        {
            AnimancerHelper = new AnimatorHelper(Animancer);
            dataBase = new();
        }
        private void Start()
        {
            CharacterActor = GetComponentInParent<CharacterActor>();

            player = GetComponentInParent<Player>();

            InitState();

            SetStateMachineData("CombatEntity", player.CombatEntity);


            stateMachine.Start();
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
            stateMachine = new(CharacterActor, characterBrain);
            stateMachine.ExternalReference = camera.transform;
            stateMachine.animator = CharacterActor.GetComponentInChildren<Animator>();
            stateMachine.database = dataBase;
            stateMachine.animancer = AnimancerHelper;

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

            stateMachine.AddState(movementState);
            stateMachine.AddState(crouchMovementState);
            stateMachine.AddState(jumpMovement);
            stateMachine.AddState(climbMovementState);


        }
        void InitLoginState()
        {
            loginMachine = new(CharacterActor, characterBrain);
            loginMachine.movementStateMachine = stateMachine;
            loginMachine.animancer = AnimancerHelper;
            var emptyState = new CharacterEmptyLoginState();
            var interactState = new CharacterInteractionState()
            {
                interactAnimations = animatorConfig.clipAnimators
            };
            loginMachine.AddState(emptyState);
            loginMachine.AddState(interactState);
            loginMachine.SetDefaultState(ECharacterLoginState.Empty);
        }
        private void Update()
        {
            stateMachine.Update();
            loginMachine.Update();
        }

        private void FixedUpdate()
        {
            stateMachine.FixUpdate();
            loginMachine.FixUpdate();
        }

        public void HandleLock()
        {
            // var movestate = controller.FindState("move") as MovementState;
            // movestate?.HandleLockEnemy(camera.currentLockOnTarget);
        }

        public void SetStateMachineData(string key, object value)
        {
            stateMachine.database.SetData(key, value);
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (CharacterActor != null)
                Gizmos.DrawLine(transform.position, transform.position + this.CharacterActor.PlanarVelocity.normalized);
        }
    }
}