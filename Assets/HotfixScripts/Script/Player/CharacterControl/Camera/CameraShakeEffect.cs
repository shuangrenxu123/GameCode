using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机震动效果，处理屏幕震动
    /// </summary>
    public class CameraShakeEffect :MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Shake;
        public float Priority { get; set; } = 200f; // 最高优先级，最后应用震动
        public bool IsActive => isActive && isShaking;

        private bool isActive = false;
        private bool isShaking = false;

        // 震动参数
        private float shakeDuration = 0f;
        private float shakeIntensity = 0f;
        private float shakeFrequency = 10f;
        private Vector3 shakeOffset = Vector3.zero;
        private float shakeTimer = 0f;
        private float noiseOffset = 0f;

        // 震动配置
        [SerializeField, LabelText("衰减速度")]
        private float dampingSpeed = 2f; // 震动衰减速度

        /// <summary>
        /// 启动震动效果
        /// </summary>
        public void StartShake(float duration, float intensity, float frequency = 10f)
        {
            if (!isActive) return;

            shakeDuration = duration;
            shakeIntensity = intensity;
            shakeFrequency = frequency;
            shakeTimer = 0f;
            isShaking = true;

            // 为每个震动实例使用不同的噪声偏移，避免重复
            noiseOffset = Random.Range(0f, 1000f);
        }

        /// <summary>
        /// 停止震动效果
        /// </summary>
        public void StopShake()
        {
            isShaking = false;
            shakeOffset = Vector3.zero;
        }

        /// <summary>
        /// 设置震动参数
        /// </summary>
        public void SetParameters(float dampingSpeed = 2f)
        {
            this.dampingSpeed = dampingSpeed;
        }

        public void Activate(CameraEffectContext context)
        {
            isActive = true;
            isShaking = false;
            shakeOffset = Vector3.zero;
        }

        public void Activate()
        {
            isActive = true;
            isShaking = false;
            shakeOffset = Vector3.zero;
        }

        public void Deactivate()
        {
            isActive = false;
            StopShake();
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive || !isShaking)
            {
                return context;
            }

            // 更新震动定时器
            shakeTimer += context.deltaTime;

            // 检查震动是否结束
            if (shakeTimer >= shakeDuration)
            {
                StopShake();
                return context;
            }

            // 计算震动进度（0-1）
            float progress = shakeTimer / shakeDuration;

            // 计算当前震动强度（带衰减）
            float currentIntensity = shakeIntensity * (1f - progress);

            // 生成Perlin噪声作为震动偏移
            float time = Time.time * shakeFrequency + noiseOffset;

            // 在不同轴向上生成不同的噪声值
            float offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * currentIntensity;
            float offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * currentIntensity;
            float offsetZ = (Mathf.PerlinNoise(time * 0.5f, time * 0.3f) - 0.5f) * 2f * currentIntensity * 0.5f; // Z轴震动较小

            shakeOffset = new Vector3(offsetX, offsetY, offsetZ);

            // 应用震动偏移到相机位置
            Vector3 shakenPosition = context.currentPosition + shakeOffset;

            // 创建修改后的上下文，应用震动效果
            var modifiedContext = new CameraEffectContext
            {
                targetCamera = context.targetCamera,
                targetTransform = context.targetTransform,
                basePosition = context.basePosition,
                baseRotation = context.baseRotation,
                baseFieldOfView = context.baseFieldOfView,
                deltaTime = context.deltaTime,
                currentPosition = shakenPosition, // 应用震动偏移的位置
                currentRotation = context.currentRotation,
                currentFieldOfView = context.currentFieldOfView,
                currentDistance = context.currentDistance
            };

            return modifiedContext;
        }

        /// <summary>
        /// 获取当前震动状态
        /// </summary>
        public bool IsShaking()
        {
            return isShaking;
        }

        /// <summary>
        /// 获取剩余震动时间
        /// </summary>
        public float GetRemainingShakeTime()
        {
            if (!isShaking) return 0f;
            return Mathf.Max(0f, shakeDuration - shakeTimer);
        }

        /// <summary>
        /// 获取当前震动强度
        /// </summary>
        public float GetCurrentShakeIntensity()
        {
            if (!isShaking) return 0f;
            float progress = shakeTimer / shakeDuration;
            return shakeIntensity * (1f - progress);
        }
    }
}