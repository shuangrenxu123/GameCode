using System.Collections.Generic;
using CharacterControllerStateMachine;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机锁定效果，处理锁定目标敌人的相机行为
    /// </summary>
    public class CameraLockOnEffect : ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.LockOn;
        public float Priority { get; set; } = 200f;
        public bool IsActive => isActive && currentLockTarget != null;

        private float lockDistance = 20f;
        private float lockEnemyMaxDistance = 30f;
        private float lockCameraMoveSpeed = 10f;
        private string lockEnemyTag = "Enemy";
        private float viewableAngle = 60f; // 可视角度范围
        private Vector3 lockOffsetPosition = Vector3.zero;
        private LayerMask lockMask;
        private Transform currentLockTarget;
        private Transform characterTransform;
        private StateManger stateManager;
        private bool isActive = false;

        /// <summary>
        /// 设置锁定参数
        /// </summary>
        public void SetParameters(float lockDistance = 20f, float lockEnemyMaxDistance = 30f, float lockCameraMoveSpeed = 10f, string lockEnemyTag = "Enemy", float viewableAngle = 60f)
        {
            this.lockDistance = lockDistance;
            this.lockEnemyMaxDistance = lockEnemyMaxDistance;
            this.lockCameraMoveSpeed = lockCameraMoveSpeed;
            this.lockEnemyTag = lockEnemyTag;
            this.viewableAngle = viewableAngle;
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

        public void Activate(CameraEffectContext context)
        {
            characterTransform = context.targetTransform;
            isActive = true;

            FindLockTarget();
        }

        public void Deactivate()
        {
            isActive = false;
            currentLockTarget = null;
        }

        public void Update(float deltaTime)
        {
            if (!isActive || currentLockTarget == null) return;

            Vector3 direction = currentLockTarget.position - characterTransform.position;
            if (direction.sqrMagnitude >= lockEnemyMaxDistance * lockEnemyMaxDistance)
            {
                currentLockTarget = null;
                if (stateManager != null) stateManager.HandleLock();
            }
        }

        public CameraEffectResult ModifyCamera(CameraEffectInput input)
        {
            if (!isActive || currentLockTarget == null) return CameraEffectResult.Default;

            Vector3 direction = (currentLockTarget.position + lockOffsetPosition) - input.basePosition;
            direction.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion smoothedRotation = Quaternion.Slerp(input.baseRotation, targetRotation, lockCameraMoveSpeed * Time.deltaTime);

            return CameraEffectResult.Rotation(smoothedRotation);
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

        public void Activate()
        {

        }
    }
}