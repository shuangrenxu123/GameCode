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

            movementState.lookingDirectionParameters.lookingDirectionMode
                = LookingDirectionParameters.LookingDirectionMode.Movement;



            var jumpMovement = new CharacterAirMovementState
            {
                database = dataBase,
                Animancer = AnimancerHelper,
                materialControl = materialControl,
                jumpAnim = animatorConfig.clipAnimators["Jump"],
                downAnim = animatorConfig.clipAnimators["JumpFall"],
                jumpEndAnim = animatorConfig.clipAnimators["JumpEnd"]
            };

            // var ladderClimb = new LadderClimbingState
            // {
            //     database = dataBase
            // };


            // var interaction = new InteractionState()
            // {
            //     database = dataBase,
            //     Animancer = this.AnimancerHelper
            // };


            // var roll = new RollState
            // {
            //     database = dataBase,
            //     Animancer = this.AnimancerHelper,
            // };

            // var Attack = new AttackState
            // {
            //     database = dataBase,
            //     Animancer = this.AnimancerHelper,
            //     animator = attackAnimator
            // };

            // var moveToladder = new StateTransition("move", "ladder");
            // var moveToladderCondition = new StateCondition_Bool("ladder", dataBase, true);

            // var InteractionTomove = new StateTransition("interaction", "move");
            // var moveToInteraction = new StateTransition("move", "interaction");
            // var InteractionTomoveCondition = new StateCondition_Bool("interaction", dataBase, false);
            // var moveToInteractionCondition = new StateCondition_Bool("interaction", dataBase, true);

            // var rollTomoveCondition = new StateCondition_Bool("roll", dataBase, false);
            // var moveTorollCondition = new StateCondition_Bool("roll", dataBase, true);
            // var rollTomove = new StateTransition("roll", "move");
            // var moveToroll = new StateTransition("move", "roll");

            // InteractionTomove.AddCondition(InteractionTomoveCondition);
            // moveToInteraction.AddCondition(moveToInteractionCondition);
            // //moveToladder.AddCondition(moveToladderCondition);
            // moveToroll.AddCondition(moveTorollCondition);
            // rollTomove.AddCondition(rollTomoveCondition);

            // dataBase.SetData("ladder", false);
            // dataBase.SetData("interaction", false);
            // dataBase.SetData("roll", false);
            // dataBase.SetData("attack", false);


            // controller.AddState(StateType.Attack, Attack);
            controller.AddState(movementState);
            controller.AddState(crouchMovementState);
            controller.AddState(jumpMovement);
            // controller.SetDefaultState(ECharacterMoveState.CrouchMove);
            ////controller.AddState("ladder", ladderClimb);
            //controller.AddState("interaction", interaction);
            //controller.AddState("roll", roll);

            // AddCondition(controller, "attack", dataBase, StateType.Walk, StateType.Attack);
            ////controller.AddTransition(moveToladder);
            //controller.AddTransition(moveToInteraction);
            //controller.AddTransition(InteractionTomove);
            //controller.AddTransition(moveToroll);
            //controller.AddTransition(rollTomove);

        }


        private void AddCondition(CharacterStateController_New controller, string name, DataBase database, StateType form, StateType to)
        {
            var cond = new StateCondition_Bool(name, database, true);
            var transition = new StateTransition<StateType>(form, to);

            var cond2 = new StateCondition_Bool(name, database, false);
            var transition2 = new StateTransition<StateType>(to, form);
            transition.AddCondition(cond);
            transition2.AddCondition(cond2);
            controller.AddTransition(transition);
            controller.AddTransition(transition2);
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