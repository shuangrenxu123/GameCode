using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{

    public class CameraFixPositionEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.FixPosition;

        private float priority = 150f;
        public float Priority { get => priority; set => priority = value; }

        [SerializeField, ReadOnly]
        private Vector3 fixedPosition = Vector3.zero;

        private bool isActive = false;

        // 平滑移动相关变量
        private bool isMoving = false;
        private Vector3 startPosition = Vector3.zero;
        private float moveProgress = 0f;
        private float moveDuration = 1f;

        [SerializeField, Tooltip("是否启用平滑移动")]
        private bool enableSmoothMove = true;
        public bool IsActive => isActive;

        public bool isDefaultActive => false;

        void SetFixedPosition(Vector3 position)
        {
            fixedPosition = position;
        }

        public void Activate()
        {
            isActive = true;
        }

        /// <summary>
        /// 启动平滑移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="duration">移动持续时间</param>
        public void ActivateWithSmoothMove(Vector3 targetPosition, float duration = 1f)
        {
            SetFixedPosition(targetPosition);
            isActive = true;


            StartSmoothMove(duration);


        }

        /// <summary>
        /// 开始平滑移动
        /// </summary>
        /// <param name="duration">移动持续时间，-1表示使用默认值</param>
        void StartSmoothMove(float duration)
        {
            if (!enableSmoothMove || duration < 0)
            {
                return;
            }

            isMoving = true;
            moveProgress = 0f;
            moveDuration = duration;

        }

        public void Deactivate()
        {
            isActive = false;
            StopSmoothMove();
        }

        /// <summary>
        /// 停止平滑移动
        /// </summary>
        public void StopSmoothMove()
        {
            isMoving = false;
            moveProgress = 0f;
        }


        /// <summary>
        /// 检查是否正在进行平滑移动
        /// </summary>
        public bool IsMoving()
        {
            return isMoving && isActive;
        }

        /// <summary>
        /// 获取当前移动进度（0-1）
        /// </summary>
        public float GetMoveProgress()
        {
            return isMoving ? moveProgress : 0f;
        }

        /// <summary>
        /// 获取剩余移动时间
        /// </summary>
        public float GetRemainingMoveTime()
        {
            if (!isMoving) return 0f;
            return Mathf.Max(0f, moveDuration - (moveProgress * moveDuration));
        }

        /// <summary>
        /// 立即完成平滑移动
        /// </summary>
        public void CompleteMoveImmediately()
        {
            if (isMoving)
            {
                isMoving = false;
                moveProgress = 1f;
            }
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive)
            {
                return context;
            }

            // 创建修改后的上下文
            CameraEffectContext modifiedContext = context;

            // 处理平滑移动逻辑
            if (enableSmoothMove && isMoving)
            {
                // 第一次调用时记录起始位置
                if (moveProgress == 0f)
                {
                    startPosition = context.currentPosition;
                }

                // 更新移动进度
                moveProgress += context.deltaTime / moveDuration;

                // 检查是否完成移动
                if (moveProgress >= 1f)
                {
                    // 移动完成，设置到最终位置
                    isMoving = false;
                    modifiedContext.currentPosition = fixedPosition;
                }
                else
                {
                    // 使用Lerp进行平滑插值
                    modifiedContext.currentPosition = Vector3.Lerp(startPosition, fixedPosition, moveProgress);
                }
            }
            else
            {
                // 传统模式：瞬间设置位置
                modifiedContext.currentPosition = fixedPosition;
            }

            // 不修改旋转
            modifiedContext.currentRotation = context.currentRotation;

            // 不修改视野角度
            modifiedContext.currentFieldOfView = context.currentFieldOfView;

            return modifiedContext;
        }
    }
}
