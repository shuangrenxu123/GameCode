using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : CharacterControlStateBase
{
    public enum RollEnum
    {

        /// <summary>
        /// ��ɫ����ķ���
        /// </summary>
        FacingDirection,
        /// <summary>
        /// ���������ķ���
        /// </summary>
        CameraDirection,
        /// <summary>
        /// �û������ֵ
        /// </summary>
        InputDirection,
    }
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        if (RuntimeAnimatorController != null)
        {
            CharacterStateController.Animator.runtimeAnimatorController = RuntimeAnimatorController;
        }
    }
    public override void FixUpdate()
    {
        AnimatorStateInfo info = CharacterStateController.Animator.GetCurrentAnimatorStateInfo(0);

        if (info.normalizedTime >= 1.0f)
        {
            database.SetData<bool>("roll", false);
        }
    }
}