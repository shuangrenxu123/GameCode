using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机旋转效果，处理相机的Yaw和Pitch旋转
    /// </summary>
    public class CameraRotationEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.Rotation;
        public float Priority { get; set; } = 70;
        public bool IsActive => isActive;

        [SerializeField, LabelText("水平旋转速度")]
        private float yawSpeed = 180f;
        [SerializeField, LabelText("垂直旋转速度")]
        private float pitchSpeed = 180f;
        [SerializeField, LabelText("最大俯仰角度")]
        private float maxPitchAngle = 80f;
        [SerializeField, LabelText("最小俯仰角度")]
        private float minPitchAngle = -80f;

        private float deltaYaw = 0f;
        private float deltaPitch = 0f;
        private Vector3 lerpedCharacterUp = Vector3.up;
        private Vector3 previousLerpedCharacterUp = Vector3.up;
        private CharacterActor characterActor;
        private Transform viewReference;
        private bool isActive = false;

        /// <summary>
        /// 设置旋转参数
        /// </summary>
        public void SetParameters(CharacterActor character, float yawSpeed = 180f, float pitchSpeed = 180f, float maxPitchAngle = 80f, float minPitchAngle = 80f)
        {
            this.yawSpeed = yawSpeed;
            this.pitchSpeed = pitchSpeed;
            this.maxPitchAngle = maxPitchAngle;
            this.minPitchAngle = minPitchAngle;
            this.characterActor = character;
        }

        /// <summary>
        /// 设置输入增量（通常由输入系统调用）
        /// </summary>
        public void SetInputDelta(float deltaYaw, float deltaPitch)
        {
            this.deltaYaw = deltaYaw;
            this.deltaPitch = deltaPitch;
        }

        public void Activate(CameraEffectContext context)
        {
            isActive = true;

            previousLerpedCharacterUp = context.targetTransform.up;
            lerpedCharacterUp = previousLerpedCharacterUp;

            // 重置输入增量，确保从零开始
            deltaYaw = 0f;
            deltaPitch = 0f;
        }

        public void Activate()
        {
            isActive = true;
            // 重置输入增量，确保从零开始
            deltaYaw = 0f;
            deltaPitch = 0f;
        }

        public void Deactivate()
        {
            isActive = false;
        }


        public void UpdateWithCameraTransform(Transform cameraTransform)
        {
            if (!isActive || characterActor == null || cameraTransform == null) return;

            // 更新角色Up向量
            Vector3 currentUp = characterActor.Up;

            // 计算Up向量的变化
            Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, currentUp);
            previousLerpedCharacterUp = currentUp;
            lerpedCharacterUp = currentUp;

            // 应用Up向量变化到相机旋转
            cameraTransform.rotation = deltaRotation * cameraTransform.rotation;
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive || characterActor == null)
            {
                return context;
            }

            // Update逻辑开始：更新角色Up向量
            UpdateCharacterUp();

            // 计算朝向（看向玩家，使用角色位置）
            float targetHeight = characterActor.BodySize.y * 0.5f;
            Vector3 lookAtPoint = characterActor.transform.position + characterActor.Up * targetHeight;
            Vector3 direction = (lookAtPoint - characterActor.transform.position).normalized;

            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // 应用用户输入旋转（基于当前旋转）
            Quaternion targetRotation = CalculateTargetRotation(baseRotation, context.baseRotation);

            // 修改上下文中的旋转，直接设置当前处理的旋转
            var modifiedContext = new CameraEffectContext
            {
                targetCamera = context.targetCamera,
                targetTransform = context.targetTransform,
                basePosition = context.basePosition,
                baseRotation = context.baseRotation,
                baseFieldOfView = context.baseFieldOfView,
                deltaTime = context.deltaTime,
                currentPosition = context.currentPosition,
                currentRotation = targetRotation, // 直接设置当前处理的旋转
                currentFieldOfView = context.currentFieldOfView,
                currentDistance = context.currentDistance // 保持距离不变
            };

            return modifiedContext;
        }

        private void UpdateCharacterUp()
        {
            if (characterActor == null) return;
            lerpedCharacterUp = characterActor.Up;
        }

        private Quaternion CalculateTargetRotation(Quaternion baseRotation, Quaternion currentRotation)
        {
            // 获取当前欧拉角，确保Z轴为0
            Vector3 currentEuler = currentRotation.eulerAngles;
            currentEuler.z = 0f; // 强制Z轴为0
            Quaternion targetRotation = Quaternion.Euler(currentEuler);

            // 应用Yaw旋转（水平旋转）- 只绕Y轴
            if (Mathf.Abs(deltaYaw) > 0.001f)
            {
                float yawAngle = deltaYaw * yawSpeed * Time.deltaTime;
                Quaternion yawRotation = Quaternion.AngleAxis(yawAngle, Vector3.up);
                targetRotation = yawRotation * targetRotation;
            }

            // 应用Pitch旋转（垂直旋转）- 只绕X轴
            if (Mathf.Abs(deltaPitch) > 0.001f)
            {
                float pitchAngle = deltaPitch * pitchSpeed * Time.deltaTime;
                Vector3 rightAxis = targetRotation * Vector3.right;
                Quaternion pitchRotation = Quaternion.AngleAxis(pitchAngle, rightAxis);
                targetRotation = pitchRotation * targetRotation;
            }

            targetRotation = ClampRotation(targetRotation);

            return targetRotation;
        }

        private Quaternion ClampRotation(Quaternion rotation)
        {
            Vector3 eulerAngles = rotation.eulerAngles;

            // 规范化角度到-180到180范围，避免四元数eulerAngles的360度跳变
            if (eulerAngles.x > 180f) eulerAngles.x -= 360f;
            if (eulerAngles.y > 180f) eulerAngles.y -= 360f;
            if (eulerAngles.z > 180f) eulerAngles.z -= 360f;

            // 强制Z轴为0，确保不发生翻滚旋转
            eulerAngles.z = 0f;

            // 使用小容差值防止浮点精度问题导致的闪烁
            float tolerance = 0.1f;
            float clampedX = Mathf.Clamp(eulerAngles.x, minPitchAngle, maxPitchAngle);

            // 如果角度在容差范围内，使用平滑插值过渡
            if (Mathf.Abs(eulerAngles.x - clampedX) < tolerance)
            {
                eulerAngles.x = Mathf.Lerp(eulerAngles.x, clampedX, 0.5f);
            }
            else
            {
                eulerAngles.x = clampedX;
            }

            // 重新创建四元数，确保Z轴为0
            return Quaternion.Euler(eulerAngles);
        }
    }
}