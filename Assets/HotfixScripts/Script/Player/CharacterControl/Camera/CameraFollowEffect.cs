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

        public void Update(float deltaTime)
        {
            if (!isActive || characterActor == null)
                return;

            characterPosition = characterActor.transform.position;

            float targetHeight = characterActor.BodySize.y * 0.8f; // 使用80%高度作为头部位置
            lerpedHeight = Mathf.Lerp(lerpedHeight, targetHeight, heightLerpSpeed * deltaTime);
            UpdateCharacterUp(deltaTime);
        }

        public CameraEffectResult ModifyCamera(CameraEffectInput input)
        {
            if (!isActive || characterActor == null) return CameraEffectResult.Default;

            // 获取缩放效果的当前距离
            float distance = GetCurrentDistance(input);

            // 只计算目标位置，不涉及朝向
            Vector3 targetPosition = characterPosition + characterActor.Up * lerpedHeight +
                                   characterActor.transform.TransformDirection(offsetFromHead);

            // 应用缩放距离：相机位置 = 目标位置 - 朝向方向 * 距离
            Vector3 backDirection = input.baseRotation * Vector3.forward;
            Vector3 cameraPosition = targetPosition - backDirection * distance;

            // 只返回位置，不修改朝向或FOV
            return CameraEffectResult.Position(cameraPosition);
        }

        private void UpdateCharacterUp(float deltaTime)
        {
            if (characterActor == null)
                return;

            lerpedCharacterUp = characterActor.Up;
            Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
            previousLerpedCharacterUp = lerpedCharacterUp;
        }

        private float GetCurrentDistance(CameraEffectInput input)
        {
            // 从活跃效果中查找缩放效果
            if (input.activeEffects != null)
            {
                foreach (var effect in input.activeEffects)
                {
                    if (effect.EffectType == CameraEffectType.Zoom && effect.IsActive)
                    {
                        return (effect as CameraZoomEffect)?.GetCurrentDistance() ?? 5f;
                    }
                }
            }

            // 如果没有找到缩放效果，返回默认距离
            return 5f;
        }
    }
}