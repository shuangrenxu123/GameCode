using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 相机效果管理器，负责管理和协调所有相机效果
    /// </summary>
    public class CameraEffectManager
    {
        readonly List<ICameraEffect> m_ActiveEffects = new List<ICameraEffect>();
        readonly Dictionary<CameraEffectType, ICameraEffect> m_EffectLookup = new();
        bool m_IsEnabled = true;

        /// <summary>
        /// 激活效果的数量
        /// </summary>
        public int ActiveEffectCount => m_ActiveEffects.Count;

        /// <summary>
        /// 所有激活效果的只读列表
        /// </summary>
        public IReadOnlyList<ICameraEffect> ActiveEffects => m_ActiveEffects;

        /// <summary>
        /// 添加效果到管理器
        /// </summary>
        public void AddEffect(ICameraEffect effect)
        {
            if (effect == null || m_ActiveEffects.Contains(effect))
            {
                return;
            }

            if (effect.isDefaultActive)
            {
                effect.Activate();
            }

            m_ActiveEffects.Add(effect);

            // 更新查找表
            if (!m_EffectLookup.ContainsKey(effect.EffectType))
            {
                m_EffectLookup.Add(effect.EffectType, effect);
            }

            SortEffects();
        }

        /// <summary>
        /// 获取指定类型的效果
        /// </summary>
        public T GetEffect<T>() where T : ICameraEffect
        {
            return m_ActiveEffects.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 检查是否包含指定类型的效果
        /// </summary>
        public bool HasEffect(CameraEffectType effectType)
        {
            return m_EffectLookup.ContainsKey(effectType);
        }


        /// <summary>
        /// 每帧处理所有激活效果的Context链式传递
        /// </summary>
        public CameraEffectContext ProcessEffectChain(CameraEffectContext initialContext)
        {
            if (!m_IsEnabled || m_ActiveEffects.Count == 0)
            {
                return initialContext;
            }

            CameraEffectContext currentContext = initialContext;

            // 按优先级排序处理效果（高优先级先处理）
            foreach (var effect in m_ActiveEffects.OrderBy(e => e.Priority))
            {
                if (!effect.IsActive) continue;

                // 每个效果处理并返回修改后的上下文
                currentContext = effect.ProcessEffect(currentContext);
            }

            return currentContext;
        }

        /// <summary>
        /// 按优先级排序效果
        /// </summary>
        private void SortEffects()
        {
            m_ActiveEffects.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// 设置旋转输入（用于输入系统集成）
        /// </summary>
        public void SetRotationInput(float deltaYaw, float deltaPitch)
        {
            var rotationEffect = GetEffect<CameraRotationEffect>();
            if (rotationEffect != null)
            {
                rotationEffect.SetInputDelta(deltaYaw, deltaPitch);
            }
        }

        /// <summary>
        /// 设置缩放输入（用于输入系统集成）
        /// </summary>
        public void SetZoomInput(float deltaZoom)
        {
            var zoomEffect = GetEffect<CameraZoomEffect>();
            if (zoomEffect != null)
            {
                zoomEffect.SetZoomInput(deltaZoom);
            }
        }

    }
}