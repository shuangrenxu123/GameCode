using System.Collections.Generic;
using CharacterControllerStateMachine;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机锁定效果，处理锁定目标敌人的相机行为
    /// </summary>
    public class CameraLockOnEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.LockOn;
        public float Priority { get; set; } = 80f;
        public bool IsActive => isActive && currentLockTarget != null;

        public bool isDefaultActive => false;

        private float lockDistance = 20f;
        private float lockEnemyMaxDistance = 30f;
        private float lockCameraMoveSpeed = 10f;
        private string lockEnemyTag = "Enemy";
        private float viewableAngle = 60f; // 可视角度范围
        private Vector3 lockOffsetPosition = Vector3.zero;
        private float lookDownOffset = 1.5f;
        private LayerMask lockMask;
        private Transform currentLockTarget;
        private Transform characterTransform;
        private StateManger stateManager;
        private bool isActive = false;

        /// <summary>
        /// 设置锁定参数
        /// </summary>
        public void SetParameters(float lockDistance = 20f, float lockEnemyMaxDistance = 30f, float lockCameraMoveSpeed = 10f, string lockEnemyTag = "Enemy", float viewableAngle = 60f, float lookDownOffset = 1.5f)
        {
            this.lockDistance = lockDistance;
            this.lockEnemyMaxDistance = lockEnemyMaxDistance;
            this.lockCameraMoveSpeed = lockCameraMoveSpeed;
            this.lockEnemyTag = lockEnemyTag;
            this.viewableAngle = viewableAngle;
            this.lookDownOffset = lookDownOffset;
        }

        /// <summary>
        /// 获取瞄准范围信息
        /// </summary>
        public void GetLockOnInfo(out float detectionRadius, out float maxLockDistance, out float angleRange)
        {
            detectionRadius = lockDistance;
            maxLockDistance = lockEnemyMaxDistance;
            angleRange = viewableAngle;
        }

        public void Activate()
        {
            isActive = true;
            FindLockTarget();
        }

        public void Deactivate()
        {
            isActive = false;
            currentLockTarget = null;
        }


        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive || currentLockTarget == null)
            {
                return context;
            }

            // 确保characterTransform已设置
            if (characterTransform == null)
            {
                characterTransform = context.targetTransform;
            }

            // Update逻辑开始：检查锁定目标距离
            Vector3 direction = currentLockTarget.position - characterTransform.position;
            if (direction.sqrMagnitude >= lockEnemyMaxDistance * lockEnemyMaxDistance)
            {
                currentLockTarget = null;
                if (stateManager != null) stateManager.HandleLock();
            }

            // 如果目标仍然有效，继续处理相机旋转
            if (currentLockTarget != null)
            {
                // 动态俯视：距离越远，俯视角度越大（远距离大俯视，近距离小俯视）
                float distanceToEnemy = Vector3.Distance(context.currentPosition, currentLockTarget.position);
                float dynamicOffset = -lookDownOffset * Mathf.Clamp(distanceToEnemy / lockEnemyMaxDistance, 0.5f, 2f);
                Vector3 targetLookPosition = currentLockTarget.position + lockOffsetPosition + Vector3.up * dynamicOffset;
                Vector3 lookDirection = targetLookPosition - context.currentPosition;
                lookDirection.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                Quaternion smoothedRotation = Quaternion.Slerp(context.currentRotation, targetRotation, lockCameraMoveSpeed * Time.deltaTime);

                // 创建修改后的上下文，只修改旋转，不改变位置和距离
                var modifiedContext = new CameraEffectContext
                {
                    targetCamera = context.targetCamera,
                    targetTransform = context.targetTransform,
                    basePosition = context.basePosition,
                    baseRotation = context.baseRotation,
                    baseFieldOfView = context.baseFieldOfView,
                    deltaTime = context.deltaTime,
                    currentPosition = context.currentPosition,     // 保持当前位置不变
                    currentRotation = smoothedRotation,            // 只修改旋转，朝向敌人
                    currentFieldOfView = context.currentFieldOfView,
                    currentDistance = context.currentDistance      // 保持距离不变
                };

                return modifiedContext;
            }

            return context;
        }

        private void FindLockTarget()
        {
            List<Enemy> availableTargets = new List<Enemy>();
            float shortestTargetDistance = Mathf.Infinity;

            Collider[] colliders = Physics.OverlapSphere(characterTransform.position, lockDistance, lockMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                Enemy character = colliders[i].GetComponent<Enemy>();
                if (character != null)
                {
                    Vector3 lockTargetDirection = Vector3.ProjectOnPlane(
                        (character.transform.position - characterTransform.position),
                        characterTransform.up
                    );
                    float distanceFromTargetSqr = lockTargetDirection.sqrMagnitude;
                    lockTargetDirection.Normalize();

                    // 检查是否在可视角度范围内
                    float angle = Vector3.Angle(lockTargetDirection, characterTransform.forward);
                    if (character.transform.root != characterTransform.transform.root &&
                        angle >= -viewableAngle && angle <= viewableAngle &&
                        distanceFromTargetSqr <= lockDistance * lockDistance)
                    {
                        availableTargets.Add(character);
                    }
                }
            }

            // 找到最近的目标
            for (int i = 0; i < availableTargets.Count; i++)
            {
                if (availableTargets[i].tag != lockEnemyTag)
                    continue;

                float distance = Vector3.Distance(characterTransform.position, availableTargets[i].transform.position);
                if (distance < shortestTargetDistance)
                {
                    shortestTargetDistance = distance;
                    currentLockTarget = availableTargets[i].transform;
                }
            }
        }

        /// <summary>
        /// 手动设置锁定目标
        /// </summary>
        public void SetLockTarget(Transform target)
        {
            currentLockTarget = target;
        }

        /// <summary>
        /// 清除锁定目标
        /// </summary>
        public void ClearLockTarget()
        {
            currentLockTarget = null;
        }

        /// <summary>
        /// 获取当前锁定目标
        /// </summary>
        public Transform GetCurrentLockTarget()
        {
            return currentLockTarget;
        }
    }
}