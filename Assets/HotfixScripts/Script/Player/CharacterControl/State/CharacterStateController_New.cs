using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController_New : StateMachine
{
    MovementReferenceParmeters movementReferenceParmeters = new MovementReferenceParmeters();
    public MovementReferenceParmeters MovementReferenceParmeters => movementReferenceParmeters;
    public CharacterActor CharacterActor { get; set; }
    public Animator Animator { get; set; }
    public bool UseRootMotion { get => CharacterActor.UseRootMotion; set => CharacterActor.UseRootMotion = value; }
    public bool UpdateRootPosition { get => CharacterActor.UpdateRootPosition; set => CharacterActor.UpdateRootPosition = value; }
    public bool UpdateRootRotation { get => CharacterActor.UpdateRootRotation; set => CharacterActor.UpdateRootRotation = value; }
    public CharacterBrain CharacterBrain { get; set; }
    public new CharacterControlStateBase CurrentState => (CharacterControlStateBase)base.CurrentState;
    #region MovementReference
    public Vector3 InputMovementReference => movementReferenceParmeters.InputMovementReference;
    public Transform ExternalReference
    {
        get => movementReferenceParmeters.externalReference;
        set =>movementReferenceParmeters.externalReference = value;
    }

    public MovementReferenceParmeters.MovementReferenceMode MovementReferenceMode
    {
        get => movementReferenceParmeters.movementReferenceMode;
        set => movementReferenceParmeters.movementReferenceMode = value;
    }
    public Vector3 MovementReferenceForward => movementReferenceParmeters.MovementReferenceForward;
    public Vector3 MovementReferenceRight => movementReferenceParmeters.MovementReferenceRight;
    #endregion
    void PreCharacterSimulation(float dt) =>CurrentState.PreCharacterSimulation();
    void PostCharacterSimulation(float dt) => CurrentState.PostCharacterSimulation();

    //bool CanCurrentStateOverideAnimatorController => CurrentState.OverrideAnimatorController && Animator != null && CurrentState.RuntimeAnimatorController != null;


    public override void Init()
    {
        CharacterActor.OnPreSimulation += PreCharacterSimulation;
        CharacterActor.OnPostSimulation += PostCharacterSimulation;
        if(Animator!= null )
        {
            CharacterActor.OnAnimatorIKEvent += OnAnimatorIK;
        }
        MovementReferenceParmeters.Initialize(CharacterActor);
        base.Init();
    }
    public override void Exit()
    {
        CharacterActor.OnPreSimulation -= PreCharacterSimulation;
        CharacterActor.OnPostSimulation -= PostCharacterSimulation;
        if(Animator!= null )    
            CharacterActor.OnAnimatorIKEvent -= OnAnimatorIK;
    }
    public override void FixUpdate()
    {
        MovementReferenceParmeters.UpdateData(CharacterBrain.CharacterActions.movement.value);
        CurrentState.PreCharacterSimulation();
        CurrentState.FixUpdate();
        CurrentState.PostCharacterSimulation();
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (CurrentState == null)
            return;

        CurrentState.UpdateIK(layerIndex);
    }

}
