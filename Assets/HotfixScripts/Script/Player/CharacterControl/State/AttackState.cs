using Animancer;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : CharacterControlStateBase
{
    private CharacterControlStateBase lastState;
    public CharacterWeaponAnimator animator;
    private AnimancerState state;

    private List<AnimancerEvent.Sequence> lightEvents;
    private List<AnimancerEvent.Sequence> HeaveEvents;
    private WeaponType currentWeaponType;
    public override void Init()
    {
        base.Init();
        lightEvents = new List<AnimancerEvent.Sequence>(3);
        HeaveEvents = new List<AnimancerEvent.Sequence>(3);
    }
    public override void Enter()
    {
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        lastState = (CharacterControlStateBase)CharacterStateController.lastState;
        var movestate = (lastState as MovementState);
        if ( movestate!= null)
        {
            if (movestate.isCrouched)
            {
                Debug.Log(2222);
            }
            Debug.Log("111");
        }
        state = Animancer.Play(animator.animators[0].lightAttackAnimator_OH[0]);
        state.Events.OnEnd += OnAnimatorEnd;
    }

    private void OnAnimatorEnd()
    {
        database.SetData<bool>("attack", false);
        state.Events.OnEnd -= OnAnimatorEnd;
    }
    private void CanDoCombo()
    {

    }
    private void EndDoCombo()
    {

    }
    private void OpenWeaponCollider()
    {

    }
    private void CloseWeaponCollider()
    {

    }
}
