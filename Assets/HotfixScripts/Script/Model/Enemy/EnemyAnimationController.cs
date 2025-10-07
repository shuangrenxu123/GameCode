using System;
using Animancer;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Enemy动画控制器，负责管理Enemy的动画状态和参数更新
    /// </summary>
    public class EnemyAnimationController
    {
        #region 组件引用
        private AnimancerComponent _animancer;
        private CharacterActor _characterActor;
        private CCAnimatorConfig _animatorConfig;
        private Enemy _enemy;
        #endregion

        #region 动画状态
        /// <summary>
        /// 当前播放的动画状态
        /// </summary>
        public AnimancerState CurrentAnimationState { get; private set; }

        /// <summary>
        /// 当前动画类型
        /// </summary>
        public EnemyAnimationType CurrentAnimationType { get; private set; }

        /// <summary>
        /// 上次动画类型（用于过渡检测）
        /// </summary>
        private EnemyAnimationType _previousAnimationType;
        #endregion

        #region 参数
        /// <summary>
        /// 移动速度参数
        /// </summary>
        private float _moveSpeed = 0f;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        private bool _isMoving = false;
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化动画控制器
        /// </summary>
        public void Initialize(Enemy enemy, AnimancerComponent animancer, CharacterActor characterActor, CCAnimatorConfig animatorConfig)
        {
            _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            _animancer = animancer ?? throw new ArgumentNullException(nameof(animancer));
            _characterActor = characterActor ?? throw new ArgumentNullException(nameof(characterActor));
            _animatorConfig = animatorConfig ?? throw new ArgumentNullException(nameof(animatorConfig));

            CurrentAnimationType = EnemyAnimationType.Idle;
            _previousAnimationType = EnemyAnimationType.None;

            Debug.Log("Enemy动画控制器初始化完成");
        }
        #endregion

        #region 公开接口

        /// <summary>
        /// 强制播放指定动画
        /// </summary>
        public void PlayAnimation(string animationName, float fadeDuration = 0.25f)
        {
            if (_animatorConfig == null || _animancer == null)
                return;

            try
            {
                // 尝试从配置中获取动画剪辑
                if (_animatorConfig.clipAnimators != null && _animatorConfig.clipAnimators.TryGetValue(animationName, out var clipTransition))
                {
                    CurrentAnimationState = _animancer.Play(clipTransition, fadeDuration);
                }
                else if (_animatorConfig.linearMixerAnimators != null && _animatorConfig.linearMixerAnimators.TryGetValue(animationName, out var mixerTransition))
                {
                    CurrentAnimationState = _animancer.Play(mixerTransition, fadeDuration);
                }
                else
                {
                    Debug.LogWarning($"未找到动画: {animationName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"播放动画 {animationName} 时出错: {e.Message}");
            }
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 更新攻击动画参数
        /// </summary>
        private void UpdateAttackParameters()
        {
            // 攻击动画通常是固定播放，不需要动态参数
        }

        /// <summary>
        /// 更新受伤动画参数
        /// </summary>
        private void UpdateHurtParameters()
        {
            // 受伤动画通常是固定播放，不需要动态参数
        }

        /// <summary>
        /// 更新死亡动画参数
        /// </summary>
        private void UpdateDeadParameters()
        {
            // 死亡动画通常是固定播放，不需要动态参数
        }
        #endregion
    }
}