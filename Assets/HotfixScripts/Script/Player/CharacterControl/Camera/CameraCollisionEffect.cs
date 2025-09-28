using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机碰撞检测效果，避免相机穿透障碍物
    /// </summary>
    public class CameraCollisionEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Collision;
        public float Priority { get; set; } = 25f;
        public bool IsActive => isActive;

        [SerializeField, LabelText("检测半径")]
        private float detectionRadius = 0.3f; // 优化检测半径，对于相机碰撞检测更合适
        [SerializeField, LabelText("检测层级")]
        private LayerMask layerMask = -1; // 默认检测所有层级
        [SerializeField, LabelText("影响缩放")]
        private bool collisionAffectsZoom = false;
        private CameraZoomEffect zoomEffect;
        private RaycastHit[] hitsBuffer = new RaycastHit[10];
        private RaycastHit[] validHits = new RaycastHit[10];
        private Vector3 targetPosition;
        private Vector3 cameraPosition;
        private bool isActive = false;

        /// <summary>
        /// 设置碰撞检测参数
        /// </summary>
        public void SetParameters(float detectionRadius = 0.5f, LayerMask layerMask = default, bool collisionAffectsZoom = false)
        {
            this.detectionRadius = detectionRadius;
            this.layerMask = layerMask;
            this.collisionAffectsZoom = collisionAffectsZoom;
        }

        /// <summary>
        /// 设置缩放效果引用，用于碰撞时调整距离
        /// </summary>
        public void SetZoomEffect(CameraZoomEffect zoomEffect)
        {
            this.zoomEffect = zoomEffect;
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

            targetPosition = context.targetTransform.position;
            cameraPosition = context.currentPosition; // 使用当前处理的位置而不是基础位置

            // 计算从目标位置到相机位置的方向和距离
            Vector3 displacement = cameraPosition - targetPosition;

            bool hit = DetectCollisions(ref displacement, targetPosition);

            if (hit && collisionAffectsZoom && zoomEffect != null)
            {
                // 调整缩放距离以避免碰撞
                float newDistance = displacement.magnitude;
                zoomEffect.SetDistance(newDistance);
            }

            if (hit)
            {
                // 返回调整后的位置
                Vector3 adjustedPosition = displacement;

                // 创建修改后的上下文，直接设置当前处理的位置
                var modifiedContext = new CameraEffectContext
                {
                    targetCamera = context.targetCamera,
                    targetTransform = context.targetTransform,
                    basePosition = context.basePosition,
                    baseRotation = context.baseRotation,
                    baseFieldOfView = context.baseFieldOfView,
                    deltaTime = context.deltaTime,
                    currentPosition = adjustedPosition, // 直接设置当前处理的位置
                    currentRotation = context.currentRotation,
                    currentFieldOfView = context.currentFieldOfView,
                    currentDistance = context.currentDistance // 传递距离信息
                };

                return modifiedContext;
            }

            return context;
        }

        private bool DetectCollisions(ref Vector3 displacement, Vector3 targetPosition)
        {
            // 从目标位置向相机位置发射射线检测碰撞
            Vector3 cameraPosition = targetPosition + displacement;
            Vector3 direction = Vector3.Normalize(displacement);

            // 使用SphereCast检测从目标位置到相机位置路径上的碰撞
            int hits = Physics.SphereCastNonAlloc(
                targetPosition,    // 从目标位置开始
                detectionRadius,
                direction,         // 向相机方向发射
                hitsBuffer,
                displacement.magnitude,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            // 过滤检测结果
            int validHitsNumber = 0;
            for (int i = 0; i < hits; i++)
            {
                RaycastHit hitBuffer = hitsBuffer[i];
                // 过滤结果
                if (hitBuffer.distance == 0)
                    continue;

                validHits[validHitsNumber] = hitBuffer;
                validHitsNumber++;
            }

            if (validHitsNumber == 0)
                return false;

            // 找到最近的碰撞点
            float nearestDistance = Mathf.Infinity;
            for (int i = 0; i < validHitsNumber; i++)
            {
                RaycastHit hitBuffer = validHits[i];
                if (hitBuffer.distance < nearestDistance)
                    nearestDistance = hitBuffer.distance;
            }

            // 调整位移向量长度到碰撞点前一点，避免穿透
            float safeDistance = nearestDistance - detectionRadius;
            if (safeDistance < 0) safeDistance = 0;
            displacement = Vector3.Normalize(displacement) * safeDistance;
            return true;
        }
    }
}