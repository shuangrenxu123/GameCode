using System;
using System.Collections.Generic;

namespace Fight.Number
{
    public sealed class CombatPropertySet
    {
        private readonly Dictionary<PropertyType, StatDefinition> _definitions = new Dictionary<PropertyType, StatDefinition>();
        private readonly Dictionary<PropertyType, StatSlot> _slots = new Dictionary<PropertyType, StatSlot>();
        private readonly Dictionary<PropertyType, List<PropertyType>> _dependents = new Dictionary<PropertyType, List<PropertyType>>();
        private readonly Dictionary<ResourceType, ResourceValue> _resources = new Dictionary<ResourceType, ResourceValue>();
        private readonly Dictionary<ResourceType, PropertyType> _resourceMaxBindings = new Dictionary<ResourceType, PropertyType>();
        private readonly HashSet<PropertyType> _recalculating = new HashSet<PropertyType>();
        private int _nextHandle = 1;
        private int _batchDepth;

        public event Action<PropertyType, int, int> OnStatChanged;

        /// <summary>
        /// 注册一个普通属性，并初始化基础值与上下限。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="defaultBaseValue">默认基础值。</param>
        /// <param name="minFinalValue">最终值下限。</param>
        /// <param name="maxFinalValue">最终值上限。</param>
        public void RegisterProperty(PropertyType id, int defaultBaseValue = 0, int minFinalValue = 0, int maxFinalValue = int.MaxValue)
        {
            ThrowIfPropertyRegistered(id);

            var definition = new StatDefinition(defaultBaseValue, minFinalValue, maxFinalValue);
            int normalizedBaseValue = NormalizeValue(definition, defaultBaseValue);
            var slot = new StatSlot
            {
                BaseValue = normalizedBaseValue,
                ComputedBaseValue = normalizedBaseValue,
                FinalValue = normalizedBaseValue,
                Dirty = false,
                IsDerived = false,
            };

            _definitions.Add(id, definition);
            _slots.Add(id, slot);
            EnsureDependentsList(id);
            SyncResourcesForProperty(id);
        }

        /// <summary>
        /// 注册一个派生属性，其基础值由依赖属性通过公式动态计算。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="dependencies">该派生属性依赖的属性列表。</param>
        /// <param name="formula">派生公式，输入为属性上下文，返回基础值。</param>
        /// <param name="minFinalValue">最终值下限。</param>
        /// <param name="maxFinalValue">最终值上限。</param>
        public void RegisterDerivedProperty(
            PropertyType id,
            PropertyType[] dependencies,
            Func<DerivedPropertyContext, int> formula,
            int minFinalValue = 0,
            int maxFinalValue = int.MaxValue)
        {
            ThrowIfPropertyRegistered(id);

            if (dependencies == null || dependencies.Length == 0)
            {
                throw new InvalidOperationException("Derived property must declare at least one dependency: " + id);
            }

            if (formula == null)
            {
                throw new ArgumentNullException(nameof(formula));
            }

            var dependencySet = new HashSet<PropertyType>();
            var dependencyCopy = new PropertyType[dependencies.Length];

            for (int i = 0; i < dependencies.Length; i++)
            {
                var dependency = dependencies[i];
                if (dependency == id)
                {
                    throw new InvalidOperationException("Derived property cannot depend on itself: " + id);
                }

                if (!_definitions.ContainsKey(dependency))
                {
                    throw new InvalidOperationException("Derived property dependency is not registered: " + dependency);
                }

                if (!dependencySet.Add(dependency))
                {
                    throw new InvalidOperationException("Derived property dependency is duplicated: " + dependency);
                }

                if (HasDependencyPath(dependency, id))
                {
                    throw new InvalidOperationException("Derived property dependency cycle detected: " + id);
                }

                dependencyCopy[i] = dependency;
            }

            var definition = new StatDefinition(0, minFinalValue, maxFinalValue);
            var slot = new StatSlot
            {
                Dirty = true,
                IsDerived = true,
                Dependencies = dependencyCopy,
                Formula = formula,
            };

            _definitions.Add(id, definition);
            _slots.Add(id, slot);
            EnsureDependentsList(id);

            for (int i = 0; i < dependencyCopy.Length; i++)
            {
                EnsureDependentsList(dependencyCopy[i]);
                _dependents[dependencyCopy[i]].Add(id);
            }

            if (_batchDepth == 0)
            {
                InitializeDerivedProperty(id);
            }
            else
            {
                SyncResourcesForProperty(id);
            }
        }

        /// <summary>
        /// 判断指定属性是否已注册。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <returns>已注册返回 true，否则返回 false。</returns>
        public bool IsRegistered(PropertyType id)
        {
            return _definitions.ContainsKey(id);
        }

        /// <summary>
        /// 注册一个资源对象，并绑定其最大值来源属性。
        /// </summary>
        /// <param name="resourceType">资源类型。</param>
        /// <param name="maxPropertyType">驱动资源最大值的属性类型。</param>
        /// <returns>创建并注册后的资源对象。</returns>
        public ResourceValue RegisterResource(ResourceType resourceType, PropertyType maxPropertyType)
        {
            if (_resources.ContainsKey(resourceType))
            {
                throw new InvalidOperationException("Resource already registered: " + resourceType);
            }

            var resource = new ResourceValue();
            _resources.Add(resourceType, resource);
            _resourceMaxBindings.Add(resourceType, maxPropertyType);
            SyncResourceMaxValue(resourceType, true);
            return resource;
        }

        /// <summary>
        /// 尝试获取已注册的资源对象。
        /// </summary>
        /// <param name="resourceType">资源类型。</param>
        /// <param name="resource">输出的资源对象。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        public bool TryGetResource(ResourceType resourceType, out ResourceValue resource)
        {
            return _resources.TryGetValue(resourceType, out resource);
        }

        /// <summary>
        /// 开启批处理模式。批处理期间变更只标记脏，不立即级联重算。
        /// </summary>
        public void BeginBatch()
        {
            _batchDepth++;
        }

        /// <summary>
        /// 结束一次批处理。最外层批处理结束时会统一重算所有脏属性。
        /// </summary>
        public void EndBatch()
        {
            if (_batchDepth <= 0)
            {
                throw new InvalidOperationException("EndBatch called without matching BeginBatch.");
            }

            _batchDepth--;
            if (_batchDepth == 0)
            {
                RecalculateAllDirty();
            }
        }

        /// <summary>
        /// 设置普通属性的基础值。派生属性不允许直接设置。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="value">新的基础值。</param>
        public void SetBaseValue(PropertyType id, int value)
        {
            var definition = GetDefinition(id);
            var slot = GetSlot(id);
            if (slot.IsDerived)
            {
                throw new InvalidOperationException("Cannot set base value on derived property: " + id);
            }

            value = NormalizeValue(definition, value);

            if (slot.BaseValue == value)
            {
                return;
            }

            slot.BaseValue = value;
            slot.ComputedBaseValue = value;
            MarkDirty(id);
        }

        /// <summary>
        /// 获取属性当前基础值（普通属性为 BaseValue，派生属性为公式计算值）。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <returns>属性基础值。</returns>
        public int GetBaseValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetSlot(id).ComputedBaseValue;
        }

        /// <summary>
        /// 获取属性最终值（基础值叠加所有激活 Modifier 后并做上下限裁剪）。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <returns>属性最终值。</returns>
        public int GetFinalValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetSlot(id).FinalValue;
        }

        /// <summary>
        /// 尝试获取属性最终值；若属性未注册则返回 false。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="value">输出的属性最终值。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        public bool TryGetFinalValue(PropertyType id, out int value)
        {
            if (!_definitions.ContainsKey(id))
            {
                value = default;
                return false;
            }

            value = GetFinalValue(id);
            return true;
        }

        /// <summary>
        /// 为指定属性添加一个 Modifier。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="value">修正值。</param>
        /// <param name="type">修正类型。</param>
        /// <param name="source">修正来源。</param>
        /// <returns>新增 Modifier 的句柄。</returns>
        public ModifierHandle AddModifier(PropertyType id, int value, ModifierType type, ModifierSource source)
        {
            var slot = GetSlot(id);
            var modifier = new StatModifier
            {
                Handle = _nextHandle++,
                Value = value,
                Type = type,
                Source = source,
                Active = true,
            };

            slot.Modifiers.Add(modifier);
            MarkDirty(id);
            return new ModifierHandle(modifier.Handle);
        }

        /// <summary>
        /// 更新指定 Modifier 的数值。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="handle">Modifier 句柄。</param>
        /// <param name="newValue">新的修正值。</param>
        /// <returns>更新成功返回 true，否则返回 false。</returns>
        public bool UpdateModifierValue(PropertyType id, ModifierHandle handle, int newValue)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var slot = GetSlot(id);
            for (int i = 0; i < slot.Modifiers.Count; i++)
            {
                var modifier = slot.Modifiers[i];
                if (modifier.Handle != handle.Value || modifier.Value == newValue)
                {
                    continue;
                }

                modifier.Value = newValue;
                slot.Modifiers[i] = modifier;
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除指定句柄对应的 Modifier。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="handle">Modifier 句柄。</param>
        /// <returns>移除成功返回 true，否则返回 false。</returns>
        public bool RemoveModifier(PropertyType id, ModifierHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var slot = GetSlot(id);
            for (int i = 0; i < slot.Modifiers.Count; i++)
            {
                if (slot.Modifiers[i].Handle != handle.Value)
                {
                    continue;
                }

                RemoveAtSwapBack(slot.Modifiers, i);
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置指定 Modifier 的激活状态。
        /// </summary>
        /// <param name="id">属性类型。</param>
        /// <param name="handle">Modifier 句柄。</param>
        /// <param name="isActive">是否激活。</param>
        /// <returns>设置成功返回 true，否则返回 false。</returns>
        public bool SetModifierActive(PropertyType id, ModifierHandle handle, bool isActive)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var slot = GetSlot(id);
            for (int i = 0; i < slot.Modifiers.Count; i++)
            {
                var modifier = slot.Modifiers[i];
                if (modifier.Handle != handle.Value || modifier.Active == isActive)
                {
                    continue;
                }

                modifier.Active = isActive;
                slot.Modifiers[i] = modifier;
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 按来源批量移除所有属性上的 Modifier。
        /// </summary>
        /// <param name="source">目标来源。</param>
        /// <returns>实际移除的 Modifier 数量。</returns>
        public int RemoveModifiersBySource(ModifierSource source)
        {
            bool flushImmediately = _batchDepth == 0;
            if (flushImmediately)
            {
                BeginBatch();
            }

            int removedCount = 0;

            try
            {
                foreach (var pair in _slots)
                {
                    var propertyType = pair.Key;
                    var slot = pair.Value;
                    bool removedFromCurrentSlot = false;

                    for (int modifierIndex = slot.Modifiers.Count - 1; modifierIndex >= 0; modifierIndex--)
                    {
                        if (slot.Modifiers[modifierIndex].Source != source)
                        {
                            continue;
                        }

                        RemoveAtSwapBack(slot.Modifiers, modifierIndex);
                        removedCount++;
                        removedFromCurrentSlot = true;
                    }

                    if (removedFromCurrentSlot)
                    {
                        MarkDirty(propertyType);
                    }
                }
            }
            finally
            {
                if (flushImmediately)
                {
                    EndBatch();
                }
            }

            return removedCount;
        }

        /// <summary>
        /// 若指定属性为脏，则执行一次重算并在最终值变化时触发事件。
        /// </summary>
        /// <param name="id">属性类型。</param>
        public void RecalculateIfDirty(PropertyType id)
        {
            var definition = GetDefinition(id);
            var slot = GetSlot(id);
            if (!slot.Dirty)
            {
                return;
            }

            if (!_recalculating.Add(id))
            {
                throw new InvalidOperationException("Circular derived property recalculation detected: " + id);
            }

            try
            {
                int oldValue = slot.FinalValue;
                int baseValue = ResolveBaseValue(slot);
                int finalValue = CalculateFinalValue(definition, baseValue, slot.Modifiers);

                slot.ComputedBaseValue = baseValue;
                slot.FinalValue = finalValue;
                slot.Dirty = false;

                if (oldValue != finalValue)
                {
                    SyncResourcesForProperty(id);
                    OnStatChanged?.Invoke(id, oldValue, finalValue);
                }
            }
            finally
            {
                _recalculating.Remove(id);
            }
        }

        /// <summary>
        /// 重算当前所有脏属性。
        /// </summary>
        public void RecalculateAllDirty()
        {
            foreach (var propertyType in _slots.Keys)
            {
                RecalculateIfDirty(propertyType);
            }
        }

        private void ThrowIfPropertyRegistered(PropertyType id)
        {
            if (_definitions.ContainsKey(id))
            {
                throw new InvalidOperationException("Property already registered: " + id);
            }
        }

        private void EnsureDependentsList(PropertyType id)
        {
            if (!_dependents.ContainsKey(id))
            {
                _dependents.Add(id, new List<PropertyType>());
            }
        }

        private void InitializeDerivedProperty(PropertyType id)
        {
            var definition = GetDefinition(id);
            var slot = GetSlot(id);
            int baseValue = ResolveBaseValue(slot);
            int finalValue = CalculateFinalValue(definition, baseValue, slot.Modifiers);

            slot.ComputedBaseValue = baseValue;
            slot.FinalValue = finalValue;
            slot.Dirty = false;
            SyncResourcesForProperty(id);
        }

        private int ResolveBaseValue(StatSlot slot)
        {
            if (!slot.IsDerived)
            {
                return slot.BaseValue;
            }

            for (int i = 0; i < slot.Dependencies.Length; i++)
            {
                RecalculateIfDirty(slot.Dependencies[i]);
            }

            return slot.Formula(new DerivedPropertyContext(this));
        }

        private void MarkDirty(PropertyType id)
        {
            var visited = new HashSet<PropertyType>();
            MarkDirtyRecursive(id, visited);

            if (_batchDepth == 0)
            {
                visited.Clear();
                RecalculateDirtyCascade(id, visited);
            }
        }

        private void MarkDirtyRecursive(PropertyType id, HashSet<PropertyType> visited)
        {
            if (!visited.Add(id))
            {
                return;
            }

            var slot = GetSlot(id);
            slot.Dirty = true;

            if (!_dependents.TryGetValue(id, out var dependents))
            {
                return;
            }

            for (int i = 0; i < dependents.Count; i++)
            {
                MarkDirtyRecursive(dependents[i], visited);
            }
        }

        private void RecalculateDirtyCascade(PropertyType id, HashSet<PropertyType> visited)
        {
            if (!visited.Add(id))
            {
                return;
            }

            RecalculateIfDirty(id);

            if (!_dependents.TryGetValue(id, out var dependents))
            {
                return;
            }

            for (int i = 0; i < dependents.Count; i++)
            {
                RecalculateDirtyCascade(dependents[i], visited);
            }
        }

        private bool HasDependencyPath(PropertyType start, PropertyType target)
        {
            var visited = new HashSet<PropertyType>();
            return HasDependencyPath(start, target, visited);
        }

        private bool HasDependencyPath(PropertyType current, PropertyType target, HashSet<PropertyType> visited)
        {
            if (!visited.Add(current))
            {
                return false;
            }

            if (current == target)
            {
                return true;
            }

            var slot = GetSlot(current);
            if (!slot.IsDerived)
            {
                return false;
            }

            for (int i = 0; i < slot.Dependencies.Length; i++)
            {
                if (HasDependencyPath(slot.Dependencies[i], target, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private StatDefinition GetDefinition(PropertyType id)
        {
            if (_definitions.TryGetValue(id, out var definition))
            {
                return definition;
            }

            throw new InvalidOperationException("Property is not registered: " + id);
        }

        private StatSlot GetSlot(PropertyType id)
        {
            if (_slots.TryGetValue(id, out var slot))
            {
                return slot;
            }

            throw new InvalidOperationException("Property is not registered: " + id);
        }

        private static void RemoveAtSwapBack(List<StatModifier> list, int index)
        {
            int lastIndex = list.Count - 1;
            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }

            list.RemoveAt(lastIndex);
        }

        private static int CalculateFinalValue(StatDefinition definition, int baseValue, List<StatModifier> modifiers)
        {
            int addSum = 0;
            int addPercentSum = 0;
            double multiplyFactor = 1d;
            bool hasOverride = false;
            int overrideValue = 0;

            for (int i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (!modifier.Active)
                {
                    continue;
                }

                switch (modifier.Type)
                {
                    case ModifierType.Add:
                        addSum += modifier.Value;
                        break;

                    case ModifierType.AddPercent:
                        addPercentSum += modifier.Value;
                        break;

                    case ModifierType.Multiply:
                        multiplyFactor *= modifier.Value / 100d;
                        break;

                    case ModifierType.Override:
                        hasOverride = true;
                        overrideValue = modifier.Value;
                        break;
                }
            }

            int finalValue;
            if (hasOverride)
            {
                finalValue = overrideValue;
            }
            else
            {
                double calculatedValue = (baseValue + addSum) * (1d + addPercentSum / 100d) * multiplyFactor;
                finalValue = (int)Math.Round(calculatedValue, MidpointRounding.AwayFromZero);
            }

            return NormalizeValue(definition, finalValue);
        }

        private static int NormalizeValue(StatDefinition definition, int value)
        {
            if (value < definition.MinFinalValue)
            {
                return definition.MinFinalValue;
            }

            if (value > definition.MaxFinalValue)
            {
                return definition.MaxFinalValue;
            }

            return value;
        }

        private void SyncResourcesForProperty(PropertyType propertyType)
        {
            foreach (var binding in _resourceMaxBindings)
            {
                if (binding.Value != propertyType)
                {
                    continue;
                }

                SyncResourceMaxValue(binding.Key, false);
            }
        }

        private void SyncResourceMaxValue(ResourceType resourceType, bool forceResetWhenUninitialized)
        {
            if (!_resources.TryGetValue(resourceType, out var resource))
            {
                return;
            }

            if (!_resourceMaxBindings.TryGetValue(resourceType, out var maxPropertyType))
            {
                return;
            }

            if (!TryGetFinalValue(maxPropertyType, out var maxValue))
            {
                resource.SyncMaxValue(0, false);
                return;
            }

            bool resetCurrent = forceResetWhenUninitialized && resource.MaxValue == 0 && resource.Value == 0;
            resource.SyncMaxValue(maxValue, resetCurrent);
        }
    }
}


