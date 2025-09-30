using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机平滑移动效果
    /// 支持从一个点位到另一个点位的平滑移动，并支持自定义移动曲线
    /// </summary>
    public class CameraSmoothMoveEffect : MonoBehaviour, ICameraEffect
    {
        public CameraEffectType EffectType => CameraEffectType.SmoothMove;

        private float priority = 160f;

        public float Priority { get => priority; set => priority = value; }

        private bool isActive = false;

        public bool IsActive => isActive;

        public bool isDefaultActive => false;

        // 移动状态相关变量
        private bool isMoving = false;
        private Vector3 startPosition = Vector3.zero;
        private Vector3 targetPosition = Vector3.zero;
        private float moveProgress = 0f;
        private float moveDuration = 1f;
        private AnimationCurve currentMoveCurve;

        [SerializeField, LabelText("默认移动曲线")]
        private AnimationCurve _defaultMoveCurve;

        // 暂停相关变量
        private bool isPaused = false;
        private float pausedTime = 0f;

        // 锁定位置相关变量
        private bool isLockedAtFinalPosition = false;

        /// <summary>
        /// 激活效果
        /// </summary>
        public void Activate()
        {
            isActive = true;
        }

        /// <summary>
        /// 停用效果
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
            StopMoving();
            UnlockFromFinalPosition();
        }

        /// <summary>
        /// 开始平滑移动从起点到终点
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="endPosition">结束位置</param>
        /// <param name="duration">移动持续时间（秒）</param>
        /// <param name="moveCurve">移动曲线，如果为null则使用默认曲线</param>
        public void StartSmoothMove(Vector3 startPosition, Vector3 endPosition, float duration, AnimationCurve moveCurve = null)
        {
            if (!isActive) return;

            this.startPosition = startPosition;
            this.targetPosition = endPosition;
            this.moveDuration = duration;
            this.currentMoveCurve = moveCurve ?? GetDefaultMoveCurve();

            StartMoving();
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            isMoving = false;
            moveProgress = 0f;
            isPaused = false;
            pausedTime = 0f;
            isRelativeMove = false;
            relativeMoveOffset = Vector3.zero;
            currentMoveCurve = null;
            isLockedAtFinalPosition = false;
        }

        /// <summary>
        /// 暂停移动
        /// </summary>
        public void PauseMoving()
        {
            if (isMoving && !isPaused)
            {
                isPaused = true;
                pausedTime = Time.time;
            }
        }

        /// <summary>
        /// 恢复移动
        /// </summary>
        public void ResumeMoving()
        {
            if (isMoving && isPaused)
            {
                isPaused = false;
                // 调整开始时间，补偿暂停的时间
                moveStartTime += (Time.time - pausedTime);
            }
        }

        /// <summary>
        /// 立即完成移动
        /// </summary>
        public void CompleteMoveImmediately()
        {
            if (isMoving)
            {
                isMoving = false;
                moveProgress = 1f;
                isPaused = false;
                pausedTime = 0f;
                isLockedAtFinalPosition = true;
            }
        }

        /// <summary>
        /// 解除位置锁定
        /// </summary>
        public void UnlockFromFinalPosition()
        {
            isLockedAtFinalPosition = false;
        }

        // 私有方法和变量
        private void StartMoving()
        {
            isMoving = true;
            moveProgress = 0f;
            isPaused = false;
            pausedTime = 0f;
            moveStartTime = 0;
        }

        private float moveStartTime = 0f;
        private Vector3 relativeMoveOffset = Vector3.zero;
        private bool isRelativeMove = false;

        /// <summary>
        /// 获取当前移动进度（0-1）
        /// </summary>
        public float GetMoveProgress()
        {
            return isMoving ? moveProgress : 0f;
        }

        /// <summary>
        /// 获取剩余移动时间（秒）
        /// </summary>
        public float GetRemainingMoveTime()
        {
            if (!isMoving) return 0f;
            return Mathf.Max(0f, moveDuration - (moveProgress * moveDuration));
        }

        /// <summary>
        /// 获取移动总时间（秒）
        /// </summary>
        public float GetTotalMoveTime()
        {
            return moveDuration;
        }


        /// <summary>
        /// 获取当前移动曲线
        /// </summary>
        public AnimationCurve GetCurrentMoveCurve()
        {
            return currentMoveCurve;
        }

        /// <summary>
        /// 检查是否正在移动
        /// </summary>
        public bool IsMoving()
        {
            return isMoving && isActive;
        }

        /// <summary>
        /// 检查是否暂停
        /// </summary>
        public bool IsPaused()
        {
            return isPaused && isMoving;
        }

        /// <summary>
        /// 检查是否锁定在最终位置
        /// </summary>
        public bool IsLockedAtFinalPosition()
        {
            return isLockedAtFinalPosition && isActive;
        }

        public CameraEffectContext ProcessEffect(CameraEffectContext context)
        {
            if (!isActive)
            {
                return context;
            }

            var modifiedContext = context;

            // 处理相对移动的开始位置设置
            if (isRelativeMove && isMoving && moveProgress == 0f)
            {
                startPosition = context.currentPosition;
                targetPosition = startPosition + relativeMoveOffset;
                isRelativeMove = false;
            }

            // 第一次调用时记录开始时间
            if (moveStartTime == 0f)
            {
                moveStartTime = Time.time;
            }

            // 处理移动逻辑
            if (isMoving)
            {
                // 计算移动进度
                float elapsedTime = Time.time - moveStartTime;
                moveProgress = Mathf.Clamp01(elapsedTime / moveDuration);

                // 应用曲线
                float curveProgress = currentMoveCurve?.Evaluate(moveProgress) ?? moveProgress;

                // 检查是否完成移动
                if (moveProgress >= 1f)
                {
                    // 移动完成，锁定在最终位置
                    isMoving = false;
                    isPaused = false;
                    isLockedAtFinalPosition = true;
                    modifiedContext.currentPosition = targetPosition;
                }
                else
                {
                    // 使用曲线进行平滑插值
                    modifiedContext.currentPosition = Vector3.Lerp(startPosition, targetPosition, curveProgress);
                }
            }
            // 处理锁定位置逻辑
            else if (isLockedAtFinalPosition && isActive)
            {
                // 保持在最终位置
                modifiedContext.currentPosition = targetPosition;
            }

            // 不修改旋转和视野角度
            modifiedContext.currentRotation = context.currentRotation;
            modifiedContext.currentFieldOfView = context.currentFieldOfView;

            return modifiedContext;
        }

        /// <summary>
        /// 获取默认移动曲线
        /// </summary>
        private AnimationCurve GetDefaultMoveCurve()
        {
            if (_defaultMoveCurve != null)
            {
                return _defaultMoveCurve;
            }

            // 创建一个平滑的缓动曲线
            return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }
}