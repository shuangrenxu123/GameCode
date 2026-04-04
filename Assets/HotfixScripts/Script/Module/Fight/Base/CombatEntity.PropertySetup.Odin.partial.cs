using System;
using System.Collections.Generic;
using Fight.Number;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight
{
    public sealed partial class CombatEntity
    {
        [Serializable]
        private struct PropertySetupItem
        {
            [LabelText("属性类型")]
            public PropertyType propertyType;

            [LabelText("基础值")]
            public int baseValue;
        }

        private const int DefaultPropertyMinValue = 0;
        private const int DefaultPropertyMaxValue = 999999;
        private const int DefaultMaxHpMinValue = 1;

        [Title("战斗属性配置")]
        [SerializeField]
        [TableList(AlwaysExpanded = true, DrawScrollView = true)]
        [ListDrawerSettings(Expanded = true, ShowPaging = false, NumberOfItemsPerPage = 20)]
        private List<PropertySetupItem> propertySetupItems = new List<PropertySetupItem>
        {
            new PropertySetupItem { propertyType = PropertyType.MaxHp, baseValue = 100 },
            new PropertySetupItem { propertyType = PropertyType.Attack, baseValue = 10 },
            new PropertySetupItem { propertyType = PropertyType.Defense, baseValue = 10 },
            new PropertySetupItem { propertyType = PropertyType.SpeedMultiplier, baseValue = 100 },
            new PropertySetupItem { propertyType = PropertyType.RotationMultiplier, baseValue = 100 },
        };

        [Button("校验属性配置"), PropertyOrder(-2)]
        private void ValidatePropertyConfig()
        {
            if (TryBuildPropertyConfigMap(out _, out var error))
            {
                Debug.Log($"[{nameof(CombatEntity)}] 属性配置校验通过。", this);
                return;
            }

            Debug.LogError(error, this);
        }

        [Button("运行时重放属性配置"), PropertyOrder(-1)]
        private void ApplyPropertyConfigNow()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning($"[{nameof(CombatEntity)}] 该按钮仅在 Play Mode 下生效。", this);
                return;
            }

            if (properties == null)
            {
                Debug.LogError($"[{nameof(CombatEntity)}] properties 尚未初始化。", this);
                return;
            }

            try
            {
                ApplyConfiguredPropertiesInternal(allowOverwriteRegisteredProperties: true);
                Debug.Log($"[{nameof(CombatEntity)}] 运行时属性配置重放成功。", this);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void ApplyConfiguredProperties()
        {
            ApplyConfiguredPropertiesInternal(allowOverwriteRegisteredProperties: false);
        }

        private void ApplyConfiguredPropertiesInternal(bool allowOverwriteRegisteredProperties)
        {
            if (properties == null)
            {
                throw new InvalidOperationException($"{nameof(CombatEntity)}.properties is null.");
            }

            if (!TryBuildPropertyConfigMap(out var propertyMap, out var error))
            {
                throw new InvalidOperationException(error);
            }

            properties.BeginBatch();
            try
            {
                foreach (var pair in propertyMap)
                {
                    var propertyType = pair.Key;
                    var baseValue = pair.Value;

                    if (properties.IsRegistered(propertyType))
                    {
                        if (!allowOverwriteRegisteredProperties)
                        {
                            throw new InvalidOperationException($"Property already registered when applying config: {propertyType}");
                        }

                        properties.SetBaseValue(propertyType, baseValue);
                        continue;
                    }

                    GetDefaultRange(propertyType, out var minValue, out var maxValue);
                    properties.RegisterProperty(propertyType, baseValue, minValue, maxValue);
                }
            }
            finally
            {
                properties.EndBatch();
            }
        }

        private bool TryBuildPropertyConfigMap(out Dictionary<PropertyType, int> propertyMap, out string error)
        {
            propertyMap = new Dictionary<PropertyType, int>();

            if (propertySetupItems == null || propertySetupItems.Count == 0)
            {
                error = $"[{nameof(CombatEntity)}] 属性配置为空，至少需要配置 {PropertyType.MaxHp}。";
                return false;
            }

            bool hasMaxHp = false;

            for (int i = 0; i < propertySetupItems.Count; i++)
            {
                var item = propertySetupItems[i];
                if (propertyMap.ContainsKey(item.propertyType))
                {
                    error = $"[{nameof(CombatEntity)}] 属性配置重复：{item.propertyType}（索引 {i}）。";
                    return false;
                }

                if (item.propertyType == PropertyType.MaxHp)
                {
                    hasMaxHp = true;
                }

                propertyMap.Add(item.propertyType, item.baseValue);
            }

            if (!hasMaxHp)
            {
                error = $"[{nameof(CombatEntity)}] 属性配置缺少 {PropertyType.MaxHp}，无法初始化生命资源。";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static void GetDefaultRange(PropertyType propertyType, out int minValue, out int maxValue)
        {
            maxValue = DefaultPropertyMaxValue;
            minValue = propertyType == PropertyType.MaxHp ? DefaultMaxHpMinValue : DefaultPropertyMinValue;
        }
    }
}
