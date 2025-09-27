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
        /// 路径跟随效果 - 沿预设路径移动
        /// </summary>
        PathFollowing,

        /// <summary>
        /// 动态构图效果 - 智能调整画面构图
        /// </summary>
        DynamicComposition
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

        /// <summary>
        /// 激活效果
        /// </summary>
        /// <param name="context">相机效果上下文</param>
        void Activate(CameraEffectContext context);

        /// <summary>
        /// 激活效果（无参数版本）
        /// </summary>
        void Activate();

        /// <summary>
        /// 停用效果
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 更新效果
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        void Update(float deltaTime);

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
        /// 时间增量
        /// </summary>
        public float deltaTime;

        /// <summary>
        /// 自定义参数字典
        /// </summary>
        public Dictionary<string, object> parameters;
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
    /// 相机效果结果
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

        /// <summary>
        /// 是否覆盖位置
        /// </summary>
        public bool overridePosition;

        /// <summary>
        /// 是否覆盖旋转
        /// </summary>
        public bool overrideRotation;

        /// <summary>
        /// 是否覆盖视野角度
        /// </summary>
        public bool overrideFOV;

        /// <summary>
        /// 创建一个默认结果，不修改任何参数
        /// </summary>
        public static CameraEffectResult Default =>
            new CameraEffectResult
            {
                overridePosition = false,
                overrideRotation = false,
                overrideFOV = false
            };

        /// <summary>
        /// 创建只修改位置的结果
        /// </summary>
        /// <param name="position">修改后的位置</param>
        /// <returns>效果结果</returns>
        public static CameraEffectResult Position(Vector3 position)
        {
            return new CameraEffectResult
            {
                modifiedPosition = position,
                overridePosition = true,
                overrideRotation = false,
                overrideFOV = false
            };
        }

        /// <summary>
        /// 创建只修改旋转的结果
        /// </summary>
        /// <param name="rotation">修改后的旋转</param>
        /// <returns>效果结果</returns>
        public static CameraEffectResult Rotation(Quaternion rotation)
        {
            return new CameraEffectResult
            {
                modifiedRotation = rotation,
                overridePosition = false,
                overrideRotation = true,
                overrideFOV = false
            };
        }

        /// <summary>
        /// 创建只修改视野角度的结果
        /// </summary>
        /// <param name="fov">修改后的视野角度</param>
        /// <returns>效果结果</returns>
        public static CameraEffectResult FOV(float fov)
        {
            return new CameraEffectResult
            {
                modifiedFieldOfView = fov,
                overridePosition = false,
                overrideRotation = false,
                overrideFOV = true
            };
        }
    }
}
