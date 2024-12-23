using HFSM;
using UnityEngine;

public class CharacterStateController_New : StateMachine
{
    MovementReferenceParameters movementReferenceParameters = new MovementReferenceParameters();
    public StateManger stateManger;
    public MovementReferenceParameters MovementReferenceParameters => movementReferenceParameters;
    public CharacterActor CharacterActor { get; set; }
    public Animator Animator { get; set; }
    public bool UseRootMotion { get => CharacterActor.UseRootMotion; set => CharacterActor.UseRootMotion = value; }
    public bool UpdateRootPosition { get => CharacterActor.UpdateRootPosition; set => CharacterActor.UpdateRootPosition = value; }
    public bool UpdateRootRotation { get => CharacterActor.UpdateRootRotation; set => CharacterActor.UpdateRootRotation = value; }
    public CharacterBrain CharacterBrain { get; set; }
    public new CharacterControlStateBase CurrentState => (CharacterControlStateBase)base.CurrentState;
    #region MovementReference
    public Vector3 InputMovementReference => movementReferenceParameters.InputMovementReference;
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
    #endregion
    void PreCharacterSimulation(float dt) => CurrentState.PreCharacterSimulation();
    void PostCharacterSimulation(float dt) => CurrentState.PostCharacterSimulation();

    public override void Init()
    {
        CharacterActor.OnPreSimulation += PreCharacterSimulation;
        CharacterActor.OnPostSimulation += PostCharacterSimulation;
        if (Animator != null)
        {
            CharacterActor.OnAnimatorIKEvent += OnAnimatorIK;
        }
        MovementReferenceParameters.Initialize(CharacterActor);
        base.Init();
    }
    public override void Exit()
    {
        CharacterActor.OnPreSimulation -= PreCharacterSimulation;
        CharacterActor.OnPostSimulation -= PostCharacterSimulation;
        if (Animator != null)
            CharacterActor.OnAnimatorIKEvent -= OnAnimatorIK;
    }
    public override void FixUpdate()
    {
        MovementReferenceParameters.UpdateData(CharacterBrain.CharacterActions.movement.value);
        CurrentState.FixUpdate();
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (CurrentState == null)
            return;

        CurrentState.UpdateIK(layerIndex);
    }

    public void ResetIKWeights()
    {
        CharacterActor.ResetIKWeights();
    }
}
