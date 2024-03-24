using Animancer;
using Audio;
using CharacterControlerStateMachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackState : CharacterControlStateBase
{
    private CharacterControlStateBase lastState;
    public CharacterWeaponAnimator animator;
    private AnimancerState state;

    private List<AnimancerEvent.Sequence> lightEvents;
    private List<AnimancerEvent.Sequence> HeaveEvents;
    private WeaponType currentWeaponType = 0;

    private bool canDoCombo = false;
    private int currentAnimatorIndex = 0;
    private WeaponAnimator currentWeaponAnimator;

    public override void Init()
    {
        base.Init();
        lightEvents = new List<AnimancerEvent.Sequence>(4);
        HeaveEvents = new List<AnimancerEvent.Sequence>(4);
    }
    public override void Enter()
    {
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        lastState = (CharacterControlStateBase)CharacterStateController.lastState;
        currentWeaponAnimator = animator.animators.First(x => x.type == WeaponType.None);

        PlayFirstAnimator();

    }

    private void PlayFirstAnimator()
    {
        var movestate = (lastState as MovementState);
        if (movestate == null)
        {
            return;
        }
        //�ҵ���ǰװ������������
        AddAnimatorEvent();
        //�¶׹���
        if (movestate.isCrouched)
        {
            database.SetData<bool>("attack", false);
        }
        //���¹���
        else if (CharacterActor.IsGrounded == false)
        {
            database.SetData<bool>("attack", false);
        }
        // �ܲ�����
        else if (movestate.IsRun)
        {
            state = Animancer.Play(currentWeaponAnimator.RunlightAttackAnimator_OH[currentAnimatorIndex].clip);
        }
        else
        {
            //�����ṥ������
            state = Animancer.Play(currentWeaponAnimator.lightAttackAnimator_OH[currentAnimatorIndex].clip);
            AudioManager.Instance.PlayAudio(currentWeaponAnimator.lightAttackAnimator_OH[currentAnimatorIndex].attackAirClip, AudioLayer.Sound);
            state.Events.AddRange(lightEvents[currentAnimatorIndex]);
        }
        if (state == null)
        {
            return;
        }
        state.Events.OnEnd += OnAnimatorEnd;

    }
    public override void Update()
    {
        if (CharacterActions.attack.Started)
        {
            //todo �ж����ع���
            //todo �ж����һ��
            if (currentAnimatorIndex == 2)
            {
                return;
            }
            if (canDoCombo)
            {
                state.Events.Clear();
                currentAnimatorIndex += 1;
                EndDoCombo();
                CloseWeaponCollider();
                state = Animancer.Play(currentWeaponAnimator.lightAttackAnimator_OH[currentAnimatorIndex].clip);
                AudioManager.Instance.PlayAudio(currentWeaponAnimator.lightAttackAnimator_OH[currentAnimatorIndex].attackAirClip, AudioLayer.Sound);

                state.Events.AddRange(lightEvents[currentAnimatorIndex]);
                state.Events.OnEnd += OnAnimatorEnd;
            }
        }
    }
    public override void Exit()
    {
        lightEvents.Clear();
        if (state == null)
        {
            return;
        }
        state.Events = null;
        currentAnimatorIndex = 0;
        canDoCombo = false;
    }
    private void AddAnimatorEvent()
    {
        if (CheckinWeaponType())
        {
            foreach (var clip in currentWeaponAnimator.lightAttackAnimator_OH)
            {
                var sequeue = new AnimancerEvent.Sequence(4)
                {
                    new AnimancerEvent(clip.ComboTime.x, CanDoCombo),
                    new AnimancerEvent(clip.ComboTime.y, EndDoCombo),
                    new AnimancerEvent(clip.HitTime.x,OpenWeaponCollider),
                    new AnimancerEvent(clip.HitTime.y,CloseWeaponCollider),
                };
                lightEvents.Add(sequeue);
            }
            foreach (var clip in currentWeaponAnimator.lightAttackAnimator_TH)
            {

            }
        }
        else
        {

        }

    }
    private bool CheckinWeaponType()
    {
        return true;
    }
    private void OnAnimatorEnd()
    {
        database.SetData<bool>("attack", false);
        state.Events.OnEnd -= OnAnimatorEnd;
    }

    #region Event Function
    private void CanDoCombo()
    {   
        canDoCombo = true;
    }
    private void EndDoCombo()
    {
        canDoCombo = false;
    }
    private void OpenWeaponCollider()
    {
        CharacterStateController.stateManger.player.Inventory.EquipeManager.rightCollider.EnableDamageCollider();
    }
    private void CloseWeaponCollider()
    {
        CharacterStateController.stateManger.player.Inventory.EquipeManager.rightCollider.DisableDamageCollider();

    }
    #endregion
}
