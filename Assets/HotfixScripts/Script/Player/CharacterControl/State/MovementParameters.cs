using UnityEngine;


[System.Serializable]
public class PlanarMovementParameters
{
    [Min(0f)]
    public float baseSpeedLimit = 6f;

    [Header("Run (boost)")]

    public bool canRun = true;

    [Tooltip("\"Toggle\" will activate/deactivate the action when the input is \"pressed\". On the other hand, \"Hold\" will activate the action when the input is pressed, " +
    "and deactivate it when the input is \"released\".")]
    public InputMode runInputMode = InputMode.Hold;


    [Min(0f)]
    public float boostSpeedLimit = 10f;


    [Header("静态地面设置")]
    public float stableGroundedAcceleration = 50f;
    public float stableGroundedDeceleration = 40f;
    public AnimationCurve stableGroundedAngleAccelerationBoost = AnimationCurve.EaseInOut(0f, 1f, 180f, 2f);

    [Header("Unstable grounded parameters")]
    public float unstableGroundedAcceleration = 10f;
    public float unstableGroundedDeceleration = 2f;
    public AnimationCurve unstableGroundedAngleAccelerationBoost = AnimationCurve.EaseInOut(0f, 1f, 180f, 1f);

    [Header("Not grounded parameters")]
    public float notGroundedAcceleration = 20f;
    public float notGroundedDeceleration = 5f;
    public AnimationCurve notGroundedAngleAccelerationBoost = AnimationCurve.EaseInOut(0f, 1f, 180f, 1f);


    [System.Serializable]
    public struct PlanarMovementProperties
    {
        [Tooltip("移动加速度")]
        public float acceleration;

        [Tooltip("移动减速度")]
        public float deceleration;

        [Tooltip("How fast the character reduces its current velocity.")]
        public float angleAccelerationMultiplier;

        public PlanarMovementProperties(float acceleration, float deceleration, float angleAccelerationBoost)
        {
            this.acceleration = acceleration;
            this.deceleration = deceleration;
            this.angleAccelerationMultiplier = angleAccelerationBoost;
        }
    }

}

/// <summary>
/// 平面移动相关设置
/// </summary>
[System.Serializable]
public class VerticalMovementParameters
{

    public enum UnstableJumpMode
    {
        Vertical,
        GroundNormal
    }


    #region 重力
    [Header("重力")]

    [Tooltip("它启用/禁用重力。重力值是根据跳跃顶点高度和持续时间计算的。")]
    public bool useGravity = true;
    #endregion
    [Header("Jump")]

    public bool canJump = true;

    [Space(10f)]

    [Tooltip("The gravity magnitude and the jump speed will be automatically calculated based on the jump apex height and duration. Set this to false if you want to manually " +
    "set those values.")]
    public bool autoCalculate = true;
    [Tooltip("The height reached at the apex of the jump. The maximum height will depend on the \"jumpCancellationMode\".")]
    [Min(0f)]
    public float jumpApexHeight = 2.25f;
    [Tooltip("The amount of time to reach the \"base height\" (apex).")]
    [Min(0f)]
    public float jumpApexDuration = 0.5f;
    public float jumpSpeed = 10f;
    public float gravity = 10f;
    [Space(10f)]
    [Tooltip("取消跳跃动作时降低垂直速度。")]
    public bool cancelJumpOnRelease = true;

    [Tooltip("How much the vertical velocity is reduced when canceling the jump (0 = no effect , 1 = zero velocity).")]
    [Range(0f, 1f)]
    public float cancelJumpMultiplier = 0.5f;

    [Tooltip("When canceling the jump (releasing the action), if the jump time is less than this value nothing is going to happen. Only when the timer is greater than this \"min time\" the jump will be affected.")]
    public float cancelJumpMinTime = 0.1f;

    [Tooltip("When canceling the jump (releasing the action), if the jump time is less than this value (and greater than the \"min time\") the velocity will be affected.")]
    public float cancelJumpMaxTime = 0.3f;

    [Space(10f)]

    [Tooltip("This will help to perform the jump action after the actual input has been started. This value determines the maximum time between input and ground detection.")]
    [Min(0f)]
    public float preGroundedJumpTime = 0.2f;

    [Tooltip("If the character is not grounded, and the \"not grounded time\" is less or equal than this value, the jump action will be performed as a grounded jump. This is also known as \"coyote time\".")]
    [Min(0f)]
    public float postGroundedJumpTime = 0.1f;

    /// <summary>
    /// 角色在空中可跳跃的次数
    /// </summary>
    public int availableNotGroundedJumps = 1;

    [Space(10f)]

    public bool canJumpOnUnstableGround = false;

    public void UpdateParameters()
    {
        if (autoCalculate)
        {
            gravity = (2 * jumpApexHeight) / Mathf.Pow(jumpApexDuration, 2);
            jumpSpeed = gravity * jumpApexDuration;
        }
    }

    public void OnValidate()
    {
        if (autoCalculate)
        {
            gravity = (2 * jumpApexHeight) / Mathf.Pow(jumpApexDuration, 2);
            jumpSpeed = gravity * jumpApexDuration;
        }
        else
        {
            jumpApexDuration = jumpSpeed / gravity;
            jumpApexHeight = gravity * Mathf.Pow(jumpApexDuration, 2) / 2f;

        }
    }

    /// <summary>
    /// 可否从单项平台上跳下来
    /// </summary>
    [Header("跳下来 用于单项平台")]
    public bool canJumpDown = true;

    [Space(10f)]

    public bool filterByTag = false;

    public string jumpDownTag = "JumpDown";

    [Space(10f)]

    [Min(0f)]
    public float jumpDownDistance = 0.05f;

    [Min(0f)]
    public float jumpDownVerticalVelocity = 0.5f;

}

/// <summary>
/// 蹲下的参数相关设置
/// </summary>
[System.Serializable]
public class CrouchParameters
{
    /// <summary>
    /// 是否启用下蹲功能
    /// </summary>
    public bool enableCrouch = true;
    /// <summary>
    /// 在空中也可以下蹲
    /// </summary>
    public bool notGroundedCrouch = false;

    [Tooltip("该乘数表示相对于默认高度的蹲下比例。")]
    [Min(0f)]
    public float heightRatio = 0.75f;

    [Tooltip("蹲下动作对移动速度的影响有多大？")]
    [Min(0f)]
    public float speedMultiplier = 0.3f;

    [Tooltip("\"Toggle\" will activate/deactivate the action when the input is \"pressed\". On the other hand, \"Hold\" will activate the action when the input is pressed, " +
    "and deactivate it when the input is \"released\".")]
    public InputMode inputMode = InputMode.Hold;

    [Tooltip("This field determines an anchor point in space (top, center or bottom) that can be used as a reference during size changes. " +
    "For instance, by using \"top\" as a reference, the character will shrink/grow my moving only the bottom part of the body.")]
    public SizeReferenceType notGroundedReference = SizeReferenceType.Top;


    [Min(0f)]
    public float sizeLerpSpeed = 8f;
}

[System.Serializable]
public class LookingDirectionParameters
{
    public bool changeLookingDirection = true;


    [Header("Lerp properties")]

    public float speed = 10f;

    [Header("朝向目标")]

    public LookingDirectionMode lookingDirectionMode = LookingDirectionMode.Movement;
    [Space(5f)]
    public Transform target = null;

    [Space(5f)]
    public LookingDirectionMovementSource stableGroundedLookingDirectionMode = LookingDirectionMovementSource.Input;
    public LookingDirectionMovementSource unstableGroundedLookingDirectionMode = LookingDirectionMovementSource.Velocity;
    public LookingDirectionMovementSource notGroundedLookingDirectionMode = LookingDirectionMovementSource.Input;
    public enum LookingDirectionMode
    {
        Movement,
        Target,
        ExternalReference
    }


    public enum LookingDirectionMovementSource
    {
        Velocity,
        Input
    }



}

/// <summary>
/// 按键模式
/// </summary>
public enum InputMode
{
    /// <summary>
    /// 触发
    /// </summary>
    Toggle,
    /// <summary>
    /// 长按
    /// </summary>
    Hold
}