using System.Collections.Generic;
using UnityEngine;

namespace CharacterController.Camera
{
    public enum CameraEffectType
    {
        /// <summary>
        /// 跟随效果 - 相机跟随目标移动
        /// </summary>
        Follow,

        /// <summary>
        /// 震动效果 - 屏幕震动
        /// </summary>
        Shake,

        /// <summary>
        /// 锁定效果 - 锁定目标敌人
        /// </summary>
        LockOn,

        /// <summary>
        /// 缩放效果 - 相机距离调整
        /// </summary>
        Zoom,

        /// <summary>
        /// 碰撞检测效果 - 避免相机穿透障碍物
        /// </summary>
        Collision,

        /// <summary>
        /// 过渡效果 - 场景切换过渡
        /// </summary>
        Transition,

        /// <summary>
        /// 旋转效果 - 相机旋转控制
        /// </summary>
        Rotation,

        /// <summary>
        /// 固定在某个点位
        ///  </summary>
        FixPosition,

        /// <summary>
        /// 路径跟随效果 - 沿预设路径移动
        /// </summary>
        PathFollowing,

        /// <summary>
        /// 动态构图效果 - 智能调整画面构图
        /// </summary>
       DynamicComposition,

       /// <summary>
       /// 平滑移动效果 - 从一个点位平滑移动到另一个点位，支持自定义曲线
       /// </summary>
       SmoothMove
    }


    public interface ICameraEffect
    {
        CameraEffectType EffectType { get; }

        /// <summary>
        /// 效果优先级，数值越大优先级越高
        /// </summary>
        float Priority { get; set; }

        /// <summary>
        /// 效果是否激活
        /// </summary>
        bool IsActive { get; }

        bool isDefaultActive { get; }

        /// <summary>
        /// 激活效果（无参数版本）
        /// </summary>
        void Activate();

        /// <summary>
        /// 停用效果
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 处理相机效果并返回修改后的上下文
        /// </summary>
        /// <param name="context">当前相机效果上下文</param>
        /// <returns>修改后的相机效果上下文</returns>
        CameraEffectContext ProcessEffect(CameraEffectContext context);
    }

    /// <summary>
    /// 相机效果上下文，包含激活效果所需的信息
    /// </summary>
    public struct CameraEffectContext
    {
        /// <summary>
        /// 目标相机
        /// </summary>
        public UnityEngine.Camera targetCamera;

        /// <summary>
        /// 目标变换（通常是角色）
        /// </summary>
        public Transform targetTransform;

        /// <summary>
        /// 基础位置
        /// </summary>
        public Vector3 basePosition;

        /// <summary>
        /// 基础旋转
        /// </summary>
        public Quaternion baseRotation;

        /// <summary>
        /// 基础视野角度
        /// </summary>
        public float baseFieldOfView;

        /// <summary>
        /// 时间增量
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// 当前位置（处理过程中的中间结果）
        /// </summary>
        public Vector3 currentPosition;

        /// <summary>
        /// 当前旋转（处理过程中的中间结果）
        /// </summary>
        public Quaternion currentRotation;

        /// <summary>
        /// 当前视野角度（处理过程中的中间结果）
        /// </summary>
        public float currentFieldOfView;

        /// <summary>
        /// 当前距离（处理过程中的中间结果）
        /// </summary>
        public float currentDistance;
    }

    /// <summary>
    /// 相机效果输入参数
    /// </summary>
    public struct CameraEffectInput
    {
        /// <summary>
        /// 基础位置
        /// </summary>
        public Vector3 basePosition;

        /// <summary>
        /// 基础旋转
        /// </summary>
        public Quaternion baseRotation;

        /// <summary>
        /// 基础视野角度
        /// </summary>
        public float baseFieldOfView;

        /// <summary>
        /// 目标变换
        /// </summary>
        public Transform targetTransform;

        /// <summary>
        /// 目标相机变换
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// 所有激活的效果列表
        /// </summary>
        public List<ICameraEffect> activeEffects;
    }

    /// <summary>
    /// 相机效果结果（已废弃，现在直接在CameraEffectContext中存储中间结果）
    /// </summary>
    public struct CameraEffectResult
    {
        /// <summary>
        /// 修改后的位置
        /// </summary>
        public Vector3 modifiedPosition;

        /// <summary>
        /// 修改后的旋转
        /// </summary>
        public Quaternion modifiedRotation;

        /// <summary>
        /// 修改后的视野角度
        /// </summary>
        public float modifiedFieldOfView;
    }
}
