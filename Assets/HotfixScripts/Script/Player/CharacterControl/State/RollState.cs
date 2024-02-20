using Animancer;
using UnityEngine;

public class RollState : CharacterControlStateBase
{
    #region animatorName
    public const string roll = "Roll";
    #endregion
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
    private AnimancerState state;
    public override void Init()
    {
        base.Init();
    }
    public override void Enter()
    {
        //todo �� �����泯���򷭹�
        var targetDir = CharacterStateController.InputMovementReference;
        Quaternion targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetDir);
        CharacterActor.SetYaw(targetDeltaRotation * CharacterActor.Forward);

        CharacterActor.Velocity = Vector3.zero;
        CharacterActor.SetupRootMotion(true, RootMotionVelocityType.SetVelocity, false);
        state = Animancer.Play(animators[roll]);
        state.Events.OnEnd += OnAnimatorEnd;
    }
    private void OnAnimatorEnd()
    {
        database.SetData<bool>("roll", false);
        state.Events.OnEnd -= OnAnimatorEnd;
    }
}