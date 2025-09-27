using System.Collections.Generic;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机碰撞检测效果，避免相机穿透障碍物
    /// </summary>
    public class CameraCollisionEffect : ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Collision;
        public float Priority { get; set; } = 25f;
        public bool IsActive => isActive;

        private float detectionRadius = 0.3f; // 优化检测半径，对于相机碰撞检测更合适
        private LayerMask layerMask = -1; // 默认检测所有层级
        private bool considerKinematicRigidbodies = true;
        private bool considerDynamicRigidbodies = true;
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

        public void Update(float deltaTime)
        {
            // 碰撞检测在ModifyCamera中执行
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive)
            {
                return context;
            }

            targetPosition = context.targetTransform.position;
            cameraPosition = context.basePosition;

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
                Vector3 adjustedPosition = targetPosition + displacement;

                // 创建修改后的上下文
                var modifiedContext = new CameraEffectContext
                {
                    targetCamera = context.targetCamera,
                    targetTransform = context.targetTransform,
                    basePosition = adjustedPosition, // 修改位置
                    baseRotation = context.baseRotation,
                    deltaTime = context.deltaTime,
                    parameters = new Dictionary<string, object>(context.parameters)
                };

                // 标记位置被覆盖
                modifiedContext.parameters["overridePosition"] = true;
                modifiedContext.parameters["modifiedPosition"] = adjustedPosition;

                return modifiedContext;
            }

            return context;
        }

        private bool DetectCollisions(ref Vector3 displacement, Vector3 lookAtPosition)
        {
            // 从相机位置向目标位置发射射线检测碰撞
            Vector3 cameraPosition = lookAtPosition + displacement;
            Vector3 direction = Vector3.Normalize(displacement);

            int hits = Physics.SphereCastNonAlloc(
                cameraPosition,    // 从相机位置开始
                detectionRadius,
                direction,         // 向目标位置方向发射
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
            float distance = Mathf.Infinity;
            for (int i = 0; i < validHitsNumber; i++)
            {
                RaycastHit hitBuffer = validHits[i];
                if (hitBuffer.distance < distance)
                    distance = hitBuffer.distance;
            }

            // 调整位移向量长度到碰撞点
            displacement = Vector3.Normalize(displacement) * distance;
            return true;
        }
    }
}