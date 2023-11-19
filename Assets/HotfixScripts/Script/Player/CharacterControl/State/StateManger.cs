using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManger : MonoBehaviour
{
    CharacterStateController_New controller;
    public CharacterBrain characterBrain;
    [SerializeField]
    RuntimeAnimatorController movementAnimator;
    private void Start()
    {
        controller = new CharacterStateController_New();
        controller.CharacterActor = GetComponentInParent<CharacterActor>();
        controller.CharacterBrain = characterBrain;
        controller.Animator = controller.CharacterActor.GetComponentInChildren<Animator>();

        controller.MovementReferenceMode = MovementReferenceParmeters.MovementReferenceMode.World;

        var movementState = new MovementState();
        movementState.lookingDirectionParameters.lookingDirectionMode = LookingDirectionParameters.LookingDirectionMode.Movement;
        movementState.RuntimeAnimatorController = movementAnimator;
        InitMovementParameter(movementState);

        controller.AddState("move",movementState);
        controller.Start();
    }
    public void InitMovementParameter(MovementState state)
    {
    }
    private void Update()
    {
        controller.Update();
    }
    private void FixedUpdate()
    {
        controller.FixUpdate();
    }

}
