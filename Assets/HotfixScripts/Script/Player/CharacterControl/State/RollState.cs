using Animancer;
using Network;
using PlayerInfo;
using UnityEngine;

public class RollState : CharacterControlStateBase
{
    #region animatorName
    public const string roll = "Roll";
    #endregion
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
    private AnimancerState state;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        var targetDir = CharacterStateController.InputMovementReference;
        Quaternion targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetDir);
        CharacterActor.SetYaw(targetDeltaRotation * CharacterActor.Forward);

        CharacterActor.Velocity = Vector3.zero;
        netHelper.SendAction("roll");
        CharacterActor.SetUpRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        state = Animancer.Play(animatorConfig.clipAnimators[roll]);
        state.Events.OnEnd += OnAnimatorEnd;
    }
    private void OnAnimatorEnd()
    {
        database.SetData<bool>("roll", false);
        state.Events.OnEnd -= OnAnimatorEnd;
    }
}