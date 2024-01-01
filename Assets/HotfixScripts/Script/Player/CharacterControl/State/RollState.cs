using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : CharacterControlStateBase
{
    public enum RollEnum
    {

        /// <summary>
        /// 角色朝向的方向
        /// </summary>
        FacingDirection,
        /// <summary>
        /// 摄像机朝向的方向
        /// </summary>
        CameraDirection,
        /// <summary>
        /// 用户输入的值
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