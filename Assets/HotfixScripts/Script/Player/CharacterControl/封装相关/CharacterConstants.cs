using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 该类包含了移动的中检测所有的常量，不建议更改
/// </summary>
public class CharacterConstants
{
    /// <summary>
    ///施加到接地触发器的偏移（朝向地面）。
    /// </summary>
    public const float GroundTriggerOffset = 0.05f;

    /// <summary>
    /// 此值表示跳跃字符在接触地面时保持不稳定的时间（以秒为单位）。这对于防止字符很有用
    /// 在这种不稳定的状态中停留太久了。
    /// </summary>
    public const float MaxUnstableGroundContactTime = 0.25f;

    /// <summary>
    /// 上边缘和下边缘检测射线原点之间的距离。
    /// </summary>
    public const float EdgeRaysSeparation = 0.005f;

    /// <summary>
    /// 边缘检测算法中用于射线的投射距离。
    /// </summary>
    public const float EdgeRaysCastDistance = 2f;

    /// <summary>
    ///碰撞体和碰撞形状之间的空间（由物理查询使用）。
    /// </summary>
    public const float SkinWidth = 0.005f;

    /// <summary>
    ///施加在胶囊底部（向上）的最小偏移以避免与地面接触。
    /// </summary>
    public const float ColliderMinBottomOffset = 0.1f;

    /// <summary>
    ///定义边的上法线和下限法线（来自边缘检测器）之间的最小角度。
    /// </summary>
    public const float MinEdgeAngle = 0.5f;

    /// <summary>
    /// 定义边的上法线和下限法线（来自边检测器）之间的最大角度。
    /// </summary>
    public const float MaxEdgeAngle = 170f;

    /// <summary>
    /// 定义阶跃的上法线和下限法线（来自边缘检测器）之间的最小角度。
    /// </summary>
    public const float MinStepAngle = 85f;

    /// <summary>
    /// 定义阶跃的上法线和下限法线（来自边缘检测器）之间的最大角度。
    /// </summary>
    public const float MaxStepAngle = 95f;

    /// <summary>
    /// 用于地面探测的基本距离。
    /// </summary>
    public const float GroundCheckDistance = 0.1f;

    /// <summary>
    ///可用于碰撞和滑动算法的最大迭代次数
    /// </summary>
    public const int MaxSlideIterations = 3;

    /// <summary>
    /// 模拟后使用的碰撞和滑动算法的最大可用迭代次数（动态地面处理）。
    /// </summary>
    public const int MaxPostSimulationSlideIterations = 2;

    /// <summary>
    /// 默认的重力值
    /// </summary>
    public const float DefaultGravity = 9.8f;

    /// <summary>
    /// 选择“头部接触”时考虑的最小角度值。该角度是在接触法线和“向上”矢量之间测量的。有效范围从“MinHeadContactAngle”到180度。
    /// </summary>
    public const float HeadContactMinAngle = 100f;

    /// <summary>
    /// Tolerance value considered when choosing the "wall contact". The angle is measured between the contact normal and the "Up" vector.
    /// The valid range goes from 90 - "WallContactAngleTolerance" to 90 degrees.
    /// </summary>
    public const float WallContactAngleTolerance = 10f;

    /// <summary>
    /// 用于预测角色下方地面的距离。
    /// </summary>
    public const float GroundPredictionDistance = 10f;

}