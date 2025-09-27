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
        private readonly List<ICameraEffect> m_ActiveEffects = new List<ICameraEffect>();
        private readonly Dictionary<CameraEffectType, ICameraEffect> m_EffectLookup = new Dictionary<CameraEffectType, ICameraEffect>();
        private Transform m_CameraTransform;
        private Transform m_TargetTransform;
        private UnityEngine.Camera m_TargetCamera;
        private bool m_IsEnabled = true;

        /// <summary>
        /// 是否启用效果管理器
        /// </summary>
        public bool IsEnabled
        {
            get => m_IsEnabled;
            set => m_IsEnabled = value;
        }

        /// <summary>
        /// 激活效果的数量
        /// </summary>
        public int ActiveEffectCount => m_ActiveEffects.Count;

        /// <summary>
        /// 所有激活效果的只读列表
        /// </summary>
        public IReadOnlyList<ICameraEffect> ActiveEffects => m_ActiveEffects;

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void Initialize(Transform cameraTransform, Transform targetTransform, UnityEngine.Camera targetCamera)
        {
            m_CameraTransform = cameraTransform;
            m_TargetTransform = targetTransform;
            m_TargetCamera = targetCamera;
        }

        /// <summary>
        /// 添加效果到管理器
        /// </summary>
        public void AddEffect(ICameraEffect effect, Dictionary<string, object> parameters = null)
        {
            if (effect == null || m_ActiveEffects.Contains(effect)) return;

            CameraEffectContext context = new CameraEffectContext
            {
                targetCamera = m_TargetCamera,
                targetTransform = m_TargetTransform,
                basePosition = m_CameraTransform != null ? m_CameraTransform.position : Vector3.zero,
                baseRotation = m_CameraTransform != null ? m_CameraTransform.rotation : Quaternion.identity,
                deltaTime = Time.deltaTime,
                parameters = parameters ?? new Dictionary<string, object>()
            };

            effect.Activate(context);
            m_ActiveEffects.Add(effect);

            // 更新查找表
            if (!m_EffectLookup.ContainsKey(effect.EffectType))
            {
                m_EffectLookup.Add(effect.EffectType, effect);
            }

            SortEffects();
        }

        /// <summary>
        /// 添加效果到管理器（简化版本，不传递参数）
        /// </summary>
        public void AddEffect(ICameraEffect effect)
        {
            if (effect == null || m_ActiveEffects.Contains(effect)) return;

            effect.Activate();
            m_ActiveEffects.Add(effect);

            // 更新查找表
            if (!m_EffectLookup.ContainsKey(effect.EffectType))
            {
                m_EffectLookup.Add(effect.EffectType, effect);
            }

            SortEffects();
        }

        /// <summary>
        /// 根据类型添加效果
        /// </summary>
        public void AddEffect<T>(Dictionary<string, object> parameters = null) where T : ICameraEffect, new()
        {
            var effect = new T();
            AddEffect(effect, parameters);
        }

        /// <summary>
        /// 移除指定的效果
        /// </summary>
        public void RemoveEffect(ICameraEffect effect)
        {
            if (effect == null || !m_ActiveEffects.Contains(effect)) return;

            effect.Deactivate();
            m_ActiveEffects.Remove(effect);

            // 更新查找表
            if (m_EffectLookup.ContainsKey(effect.EffectType))
            {
                m_EffectLookup.Remove(effect.EffectType);
            }
        }

        /// <summary>
        /// 根据类型移除效果
        /// </summary>
        public void RemoveEffect(CameraEffectType effectType)
        {
            if (m_EffectLookup.TryGetValue(effectType, out ICameraEffect effect))
            {
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// 获取指定类型的效果
        /// </summary>
        public T GetEffect<T>() where T : ICameraEffect
        {
            return m_ActiveEffects.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 获取指定类型的效果
        /// </summary>
        public ICameraEffect GetEffect(CameraEffectType effectType)
        {
            m_EffectLookup.TryGetValue(effectType, out ICameraEffect effect);
            return effect;
        }

        /// <summary>
        /// 检查是否包含指定类型的效果
        /// </summary>
        public bool HasEffect(CameraEffectType effectType)
        {
            return m_EffectLookup.ContainsKey(effectType);
        }

        /// <summary>
        /// 更新所有效果
        /// </summary>
        public void UpdateEffects(float deltaTime)
        {
            if (!m_IsEnabled) return;

            // 更新所有激活的效果
            for (int i = m_ActiveEffects.Count - 1; i >= 0; i--)
            {
                var effect = m_ActiveEffects[i];
                if (effect.IsActive)
                {
                    effect.Update(deltaTime);
                }
                else
                {
                    // 移除非激活的效果
                    RemoveEffect(effect);
                }
            }
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
            foreach (var effect in m_ActiveEffects.OrderByDescending(e => e.Priority))
            {
                if (!effect.IsActive) continue;

                // 每个效果处理并返回修改后的上下文
                currentContext = effect.ProcessEffect(currentContext);
            }

            return currentContext;
        }

        /// <summary>
        /// 计算组合后的效果结果
        /// </summary>
        public CameraEffectResult CalculateCombinedEffects(CameraEffectInput input)
        {
            if (!m_IsEnabled || m_ActiveEffects.Count == 0)
            {
                return CameraEffectResult.Default;
            }

            // 创建初始上下文
            CameraEffectContext context = new CameraEffectContext
            {
                targetCamera = input.cameraTransform != null ? input.cameraTransform.GetComponent<UnityEngine.Camera>() : null,
                targetTransform = input.targetTransform,
                basePosition = input.basePosition,
                baseRotation = input.baseRotation,
                deltaTime = Time.deltaTime,
                parameters = new Dictionary<string, object>()
            };

            // 按优先级排序处理效果（高优先级先处理）
            foreach (var effect in m_ActiveEffects.OrderByDescending(e => e.Priority))
            {
                if (!effect.IsActive) continue;

                // 每个效果处理并返回修改后的上下文
                context = effect.ProcessEffect(context);
            }

            // 从最终上下文构建结果
            CameraEffectResult result = CameraEffectResult.Default;
            if (context.parameters.ContainsKey("overridePosition") && (bool)context.parameters["overridePosition"])
            {
                result.modifiedPosition = (Vector3)context.parameters["modifiedPosition"];
                result.overridePosition = true;
            }

            if (context.parameters.ContainsKey("overrideRotation") && (bool)context.parameters["overrideRotation"])
            {
                result.modifiedRotation = (Quaternion)context.parameters["modifiedRotation"];
                result.overrideRotation = true;
            }

            if (context.parameters.ContainsKey("overrideFOV") && (bool)context.parameters["overrideFOV"])
            {
                result.modifiedFieldOfView = (float)context.parameters["modifiedFieldOfView"];
                result.overrideFOV = true;
            }

            return result;
        }

        /// <summary>
        /// 清除所有效果
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var effect in m_ActiveEffects)
            {
                effect.Deactivate();
            }
            m_ActiveEffects.Clear();
            m_EffectLookup.Clear();
        }


        /// <summary>
        /// 禁用指定类型的效果
        /// </summary>
        public void DisableEffect(CameraEffectType effectType)
        {
            RemoveEffect(effectType);
        }

        /// <summary>
        /// 设置效果的优先级
        /// </summary>
        public void SetEffectPriority(CameraEffectType effectType, float priority)
        {
            if (m_EffectLookup.TryGetValue(effectType, out ICameraEffect effect))
            {
                effect.Priority = priority;
                SortEffects();
            }
        }

        /// <summary>
        /// 获取效果的信息字符串（用于调试）
        /// </summary>
        public string GetDebugInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"CameraEffectManager - Active Effects: {ActiveEffectCount}");
            sb.AppendLine($"Enabled: {m_IsEnabled}");

            foreach (var effect in m_ActiveEffects.OrderByDescending(e => e.Priority))
            {
                sb.AppendLine($"- {effect.EffectType}: Priority={effect.Priority}, Active={effect.IsActive}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 按优先级排序效果
        /// </summary>
        private void SortEffects()
        {
            m_ActiveEffects.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// 验证效果优先级是否正确
        /// </summary>
        public bool ValidateEffectPriority()
        {
            for (int i = 0; i < m_ActiveEffects.Count - 1; i++)
            {
                if (m_ActiveEffects[i].Priority < m_ActiveEffects[i + 1].Priority)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取所有效果类型的列表
        /// </summary>
        public List<CameraEffectType> GetActiveEffectTypes()
        {
            return m_ActiveEffects.Select(e => e.EffectType).ToList();
        }

        /// <summary>
        /// 批量启用效果
        /// </summary>
        public void EnableEffects(params CameraEffectType[] effectTypes)
        {
            foreach (var effectType in effectTypes)
            {
            }
        }

        /// <summary>
        /// 批量禁用效果
        /// </summary>
        public void DisableEffects(params CameraEffectType[] effectTypes)
        {
            foreach (var effectType in effectTypes)
            {
                DisableEffect(effectType);
            }
        }

        /// <summary>
        /// 启用锁定效果
        /// </summary>
        public void EnableLockOnEffect(Dictionary<string, object> parameters = null)
        {
            var lockOnEffect = new CameraLockOnEffect();
            AddEffect(lockOnEffect, parameters);
        }

        /// <summary>
        /// 设置瞄准范围参数
        /// </summary>
        public void SetLockOnRange(float detectionRadius, float maxLockDistance, float angleRange = 60f)
        {
            var lockOnEffect = GetEffect<CameraLockOnEffect>();
            if (lockOnEffect != null)
            {
                lockOnEffect.SetParameters(detectionRadius, maxLockDistance, 10f, "Enemy", angleRange);
            }
        }

        /// <summary>
        /// 获取瞄准范围信息
        /// </summary>
        public void GetLockOnRangeInfo(out float detectionRadius, out float maxLockDistance, out float angleRange)
        {
            detectionRadius = 20f;
            maxLockDistance = 30f;
            angleRange = 60f;

            var lockOnEffect = GetEffect<CameraLockOnEffect>();
            if (lockOnEffect != null)
            {
                lockOnEffect.GetLockOnInfo(out detectionRadius, out maxLockDistance, out angleRange);
            }
        }

        /// <summary>
        /// 禁用锁定效果
        /// </summary>
        public void DisableLockOnEffect()
        {
            DisableEffect(CameraEffectType.LockOn);
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

        /// <summary>
        /// 设置锁定目标（用于外部锁定系统）
        /// </summary>
        public void SetLockTarget(Transform target)
        {
            var lockOnEffect = GetEffect<CameraLockOnEffect>();
            if (lockOnEffect != null)
            {
                lockOnEffect.SetLockTarget(target);
            }
        }

        /// <summary>
        /// 清除锁定目标
        /// </summary>
        public void ClearLockTarget()
        {
            var lockOnEffect = GetEffect<CameraLockOnEffect>();
            if (lockOnEffect != null)
            {
                lockOnEffect.ClearLockTarget();
            }
        }

        /// <summary>
        /// 获取当前锁定目标
        /// </summary>
        public Transform GetCurrentLockTarget()
        {
            var lockOnEffect = GetEffect<CameraLockOnEffect>();
            return lockOnEffect?.GetCurrentLockTarget();
        }
    }
}