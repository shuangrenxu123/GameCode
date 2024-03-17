using Animancer;
using Audio;
using CharacterControlerStateMachine;
using HFSM;
using System.Collections.Generic;
using UnityEngine;

public class StateManger : MonoBehaviour
{
    public CharacterStateController_New controller;
    public Player player;
    public new Camera3D camera;
    public CharacterBrain characterBrain;
    public AnimancerComponent Animancer;
    public AnimactorHelper AnimancerHelper;
    public MaterialControl materialControl;

    public AudioData moveData;

    [Header("基础移动动画")]
    [SerializeField]
    private LinearMixerTransition normalMoveAnimator;
    [SerializeField]
    private LinearMixerTransition crouchMoveAniamtor;
    [SerializeReference]
    private ITransition lockMovementAnimator;
    [SerializeField]
    private List<ClipTransition> jumpAnimator;
    [Header("交互动画")]
    [SerializeField]
    List<ClipTransition> InterctionTransition;

    [Header("翻滚动画")]
    [SerializeField]
    List<ClipTransition> rollAnimator;
    [Header("攻击动画")]
    [SerializeField]
    private CharacterWeaponAnimator attackAnimator;

    private CharacterActor CharacterActor;
    DataBase dataBase;

    private void Awake()
    {
        AnimancerHelper = new AnimactorHelper(Animancer);
         dataBase = new();
    }
    private void Start()
    {
        CharacterActor = GetComponentInParent<CharacterActor>();
        player = GetComponentInParent<Player>();
        InitState();

        SetStateMachineData("CombatEntity",player.CombatEntity);


        controller.Start();
    }

    private void InitState()
    {
        controller = new CharacterStateController_New
        {
            CharacterActor = GetComponentInParent<CharacterActor>(),
            CharacterBrain = characterBrain,
            stateManger = this,
        };
        controller.ExternalReference = camera.transform;
        controller.Animator = controller.CharacterActor.GetComponentInChildren<Animator>();
        controller.database = dataBase;
       
        var state = Animancer.States.GetOrCreate(lockMovementAnimator);
        var movementState = new MovementState
        {
            database = dataBase,
            Animancer = AnimancerHelper,
            normalMoveAnimator = normalMoveAnimator,
            crouchMoveAnimator = crouchMoveAniamtor,
            MaterialControl = materialControl,
            lockEnemyAnimator = (MixerState<Vector2>)state,
            moveAudio = moveData
        };


        movementState.AddStateAnimators(jumpAnimator);

        movementState.lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Movement;
        var ladderClimb = new LadderClimbingState
        {
            database = dataBase
        };


        var interaction = new InteractionState()
        {
            database = dataBase,
            Animancer = this.AnimancerHelper
        };
        interaction.AddStateAnimators(InterctionTransition);

        var roll = new RollState
        {
            database = dataBase,
            Animancer = this.AnimancerHelper,
        };
        roll.AddStateAnimators(rollAnimator);

        var Attack = new AttackState
        {
            database = dataBase,
            Animancer = this.AnimancerHelper,
            animator = attackAnimator
        };

        // var moveToladder = new StateTransition("move", "ladder");
        // var moveToladderCondition = new StateCondition_Bool("ladder", dataBase, true);
        //
        var InteractionTomove = new StateTransition("interaction", "move");
        var moveToInteraction = new StateTransition("move", "interaction");
        var InteractionTomoveCondition = new StateCondition_Bool("interaction", dataBase, false);
        var moveToInteractionCondition = new StateCondition_Bool("interaction", dataBase, true);

        var rollTomoveCondition = new StateCondition_Bool("roll", dataBase, false);
        var moveTorollCondition = new StateCondition_Bool("roll", dataBase, true);
        var rollTomove = new StateTransition("roll", "move");
        var moveToroll = new StateTransition("move", "roll");

        InteractionTomove.AddCondition(InteractionTomoveCondition);
        moveToInteraction.AddCondition(moveToInteractionCondition);
        //moveToladder.AddCondition(moveToladderCondition);
        moveToroll.AddCondition(moveTorollCondition);
        rollTomove.AddCondition(rollTomoveCondition);

        dataBase.SetData("ladder", false);
        dataBase.SetData("interaction", false);
        dataBase.SetData("roll", false);
        dataBase.SetData("attack", false);


        controller.AddState("attack", Attack);
        controller.AddState("move", movementState);
        //controller.AddState("ladder", ladderClimb);
        controller.AddState("interaction", interaction);
        controller.AddState("roll", roll);

        AddCondition(controller, "attack", dataBase, "move", "attack");
        //controller.AddTransition(moveToladder);
        controller.AddTransition(moveToInteraction);
        controller.AddTransition(InteractionTomove);
        controller.AddTransition(moveToroll);
        controller.AddTransition(rollTomove);

    }


    private void AddCondition(CharacterStateController_New controller, string name, DataBase database, string form, string to)
    {
        var cond = new StateCondition_Bool(name, database, true);
        var transition = new StateTransition(form, to);

        var cond2 = new StateCondition_Bool(name, database, false);
        var transition2 = new StateTransition(to, form);
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
    //todo 后面修改该函数
    public void HandleLock()
    {
        var movestate = controller.FindState("move") as MovementState;
        movestate?.HandleLockEnemy(camera.currentLockOnTarget);
    }

    public void SetStateMachineData(string key,object value)
    {
        controller.database.SetData(key,value);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (CharacterActor != null)
            Gizmos.DrawLine(transform.position, transform.position + this.CharacterActor.PlanarVelocity.normalized);
    }
}
