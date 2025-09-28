using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机跟随效果，处理相机跟随目标移动和位置计算
    /// </summary>
    public class CameraFollowEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Follow;
        public float Priority { get; set; } = 100f;
        public bool IsActive => isActive;

        public bool isDefaultActive => true;

        [SerializeField, LabelText("高度插值速度")]
        private float heightLerpSpeed = 10f;
        private Vector3 offsetFromHead = Vector3.zero;
        private CharacterActor characterActor;
        private Vector3 lerpedCharacterUp = Vector3.up;
        private float lerpedHeight;
        private Vector3 previousLerpedCharacterUp = Vector3.up;
        private Vector3 characterPosition;
        private bool isActive = false;

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
                characterActor = context.targetTransform.GetComponentInBranch<CharacterActor>();
                return context;
            }

            // Update逻辑开始：更新角色位置和高度插值
            characterPosition = characterActor.transform.position;
            float targetHeight = characterActor.BodySize.y * 0.8f; // 使用80%高度作为头部位置
            lerpedHeight = Mathf.Lerp(lerpedHeight, targetHeight, heightLerpSpeed * context.deltaTime);
            UpdateCharacterUp(context.deltaTime);

            // 使用上下文中的当前距离（由ZoomEffect提供）
            float distance = context.currentDistance;

            // 只计算目标位置，不涉及朝向
            Vector3 targetPosition = characterPosition + characterActor.Up * lerpedHeight +
                                   characterActor.transform.TransformDirection(offsetFromHead);

            // 应用缩放距离：相机位置 = 目标位置 - 朝向方向 * 距离
            // 使用当前处理链中的旋转（context.currentRotation）而不是基础旋转，确保位置和旋转同步
            Vector3 backDirection = context.currentRotation * Vector3.forward;
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
                currentFieldOfView = context.currentFieldOfView,
                currentDistance = context.currentDistance // 保持距离不变
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
    }
}