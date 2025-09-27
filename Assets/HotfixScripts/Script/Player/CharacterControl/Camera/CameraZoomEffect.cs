using System.Collections.Generic;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机缩放效果，处理相机距离调整
    /// </summary>
    public class CameraZoomEffect : ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Zoom;
        public float Priority { get; set; } = 60f;
        public bool IsActive => isActive;

        float zoomInOutSpeed = 40f;
        float zoomInOutLerpSpeed = 5f;
        float minZoom = 2f;
        float maxZoom = 12f;
        float currentDistanceToTarget;
        float smoothedDistanceToTarget;
        float deltaZoom = 0f;
        bool isActive = false;

        /// <summary>
        /// 设置缩放参数
        /// </summary>
        public void SetParameters(float zoomInOutSpeed = 40f, float zoomInOutLerpSpeed = 5f, float minZoom = 2f, float maxZoom = 12f, float initialDistance = 5f)
        {
            this.zoomInOutSpeed = zoomInOutSpeed;
            this.zoomInOutLerpSpeed = zoomInOutLerpSpeed;
            this.minZoom = minZoom;
            this.maxZoom = maxZoom;
            this.currentDistanceToTarget = initialDistance;

            if (currentDistanceToTarget == 0)
                currentDistanceToTarget = maxZoom;
            smoothedDistanceToTarget = currentDistanceToTarget;
        }

        /// <summary>
        /// 设置缩放输入增量
        /// </summary>
        public void SetZoomInput(float deltaZoom)
        {
            this.deltaZoom = deltaZoom;
        }

        public void Activate(CameraEffectContext context)
        {
            isActive = true;
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
        }


        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive)
            {
                return context;
            }

            // Update逻辑开始：更新缩放距离
            // 更新目标距离
            currentDistanceToTarget += deltaZoom * zoomInOutSpeed * context.deltaTime;
            currentDistanceToTarget = Mathf.Clamp(currentDistanceToTarget, minZoom, maxZoom);

            // 平滑过渡到目标距离
            smoothedDistanceToTarget = Mathf.Lerp(smoothedDistanceToTarget, currentDistanceToTarget, zoomInOutLerpSpeed * context.deltaTime);

            // 计算缩放后的相机位置
            Vector3 forwardDirection = context.baseRotation * Vector3.forward;
            Vector3 zoomOffset = -forwardDirection * smoothedDistanceToTarget;
            Vector3 newPosition = context.basePosition + zoomOffset;

            // 创建修改后的上下文，直接设置当前处理的位置
            var modifiedContext = new CameraEffectContext
            {
                targetCamera = context.targetCamera,
                targetTransform = context.targetTransform,
                basePosition = context.basePosition,
                baseRotation = context.baseRotation,
                baseFieldOfView = context.baseFieldOfView,
                deltaTime = context.deltaTime,
                currentPosition = newPosition, // 直接设置当前处理的位置
                currentRotation = context.currentRotation,
                currentFieldOfView = context.currentFieldOfView
            };

            return modifiedContext;
        }

        /// <summary>
        /// 获取当前平滑后的距离值，其他效果可以使用这个值
        /// </summary>
        public float GetCurrentDistance()
        {
            return smoothedDistanceToTarget;
        }

        /// <summary>
        /// 直接设置距离（用于碰撞检测等效果影响缩放）
        /// </summary>
        public void SetDistance(float distance)
        {
            currentDistanceToTarget = distance;
            smoothedDistanceToTarget = distance;
        }
    }
}