using System.Collections.Generic;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机跟随效果，处理相机跟随目标移动和位置计算
    /// </summary>
    public class CameraFollowEffect : ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Follow;
        public float Priority { get; set; } = 25f;
        public bool IsActive => isActive;

        private float heightLerpSpeed = 10f;
        private Vector3 offsetFromHead = Vector3.zero;
        private CharacterActor characterActor;
        private Vector3 lerpedCharacterUp = Vector3.up;
        private float lerpedHeight;
        private Vector3 previousLerpedCharacterUp = Vector3.up;
        private Vector3 characterPosition;
        private bool isActive = false;

        /// <summary>
        /// 设置跟随参数
        /// </summary>
        public void SetParameters(float heightLerpSpeed, CharacterActor actor, Vector3 offsetFromHead = default)
        {
            this.heightLerpSpeed = heightLerpSpeed;
            this.offsetFromHead = offsetFromHead;
            this.characterActor = actor;
        }

        public void Activate(CameraEffectContext context)
        {
            characterActor = context.targetTransform.GetComponentInBranch<CharacterActor>();

            isActive = true;

            characterPosition = context.targetTransform.position;
            previousLerpedCharacterUp = context.targetTransform.up;
            lerpedCharacterUp = previousLerpedCharacterUp;
            lerpedHeight = characterActor?.BodySize.y ?? 1.7f;
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
            if (!isActive || characterActor == null)
            {
                return context;
            }

            // Update逻辑开始：更新角色位置和高度插值
            characterPosition = characterActor.transform.position;
            float targetHeight = characterActor.BodySize.y * 0.8f; // 使用80%高度作为头部位置
            lerpedHeight = Mathf.Lerp(lerpedHeight, targetHeight, heightLerpSpeed * context.deltaTime);
            UpdateCharacterUp(context.deltaTime);

            // 获取缩放效果的当前距离
            float distance = GetCurrentDistance(context);

            // 只计算目标位置，不涉及朝向
            Vector3 targetPosition = characterPosition + characterActor.Up * lerpedHeight +
                                   characterActor.transform.TransformDirection(offsetFromHead);

            // 应用缩放距离：相机位置 = 目标位置 - 朝向方向 * 距离
            Vector3 backDirection = context.baseRotation * Vector3.forward;
            Vector3 cameraPosition = targetPosition - backDirection * distance;

            // 创建修改后的上下文，直接设置当前处理的位置
            var modifiedContext = new CameraEffectContext
            {
                targetCamera = context.targetCamera,
                targetTransform = context.targetTransform,
                basePosition = context.basePosition,
                baseRotation = context.baseRotation,
                baseFieldOfView = context.baseFieldOfView,
                deltaTime = context.deltaTime,
                currentPosition = cameraPosition, // 直接设置当前处理的位置
                currentRotation = context.currentRotation,
                currentFieldOfView = context.currentFieldOfView
            };

            return modifiedContext;
        }

        private void UpdateCharacterUp(float deltaTime)
        {
            if (characterActor == null)
                return;

            lerpedCharacterUp = characterActor.Up;
            Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
            previousLerpedCharacterUp = lerpedCharacterUp;
        }

        private float GetCurrentDistance(CameraEffectContext context)
        {
            // 从缩放效果的当前距离获取（需要通过效果管理器获取）
            // 这里暂时返回默认距离，实际实现需要在CameraFollowEffect中添加对缩放效果的引用
            return 5f;
        }
    }
}