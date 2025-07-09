using Animancer;
using Audio;
using Character.Controller.MoveState;
using Character.Controller.State;
using CharacterControllerStateMachine;
using HFSM;
using UnityEngine;

namespace CharacterControllerStateMachine
{
    public class StateManger : MonoBehaviour
    {
        public CharacterMovementStateMachine controller;
        public Player player;
        public new Camera3D camera;
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

        private CharacterActor CharacterActor;
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


            controller.Start();
        }

        private void InitState()
        {
            controller = new(CharacterActor, characterBrain);
            controller.ExternalReference = camera.transform;
            controller.animator = CharacterActor.GetComponentInChildren<Animator>();
            controller.database = dataBase;

            var movementState = new CharacterNormalMovementState
            {
                database = dataBase,
                Animancer = AnimancerHelper,
                materialControl = materialControl,
                currentAnimator = animatorConfig.linearMixerAnimators["NormalMove"]
            };

            movementState.lookingDirectionParameters.lookingDirectionMode
                = LookingDirectionParameters.LookingDirectionMode.Movement;

            var crouchMovementState = new CharacterCrouchMovementState
            {
                database = dataBase,
                Animancer = AnimancerHelper,
                materialControl = materialControl,
                currentAnimator = animatorConfig.linearMixerAnimators["CrouchMove"]
            };

            var jumpMovement = new CharacterAirMovementState
            {
                database = dataBase,
                Animancer = AnimancerHelper,
                materialControl = materialControl,
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

            controller.AddState(movementState);
            controller.AddState(crouchMovementState);
            controller.AddState(jumpMovement);
            controller.AddState(climbMovementState);
            // controller.SetDefaultState(ECharacterMoveState.Climb);
        }

        private void Update()
        {
            controller.Update();
        }

        private void FixedUpdate()
        {
            controller.FixUpdate();
        }

        public void HandleLock()
        {
            // var movestate = controller.FindState("move") as MovementState;
            // movestate?.HandleLockEnemy(camera.currentLockOnTarget);
        }

        public void SetStateMachineData(string key, object value)
        {
            controller.database.SetData(key, value);
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (CharacterActor != null)
                Gizmos.DrawLine(transform.position, transform.position + this.CharacterActor.PlanarVelocity.normalized);
        }
    }
}