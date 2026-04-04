using System;
using System.Collections.Generic;

partial
namespace Fight.Number
{
    public sealed class CombatPropertySet
    {
        private readonly Dictionary<PropertyType, StatNode> _stats = new Dictionary<PropertyType, StatNode>();
        private readonly Dictionary<ResourceType, ResourceNode> _resources = new Dictionary<ResourceType, ResourceNode>();

        private readonly HashSet<PropertyType> _recalculating = new HashSet<PropertyType>();
        private int _nextHandle = 1;
        private int _batchDepth;

        public event Action<PropertyType, int, int> OnStatChanged;

        /// <summary>
        /// 注册一个普通属性，并初始化基础值与上下限。
        /// </summary>
        public void RegisterProperty(PropertyType id, int defaultBaseValue = 0, int minFinalValue = 0, int maxFinalValue = int.MaxValue)
        {
            ThrowIfPropertyRegistered(id);

            var definition = new StatDefinition(defaultBaseValue, minFinalValue, maxFinalValue);
            int normalizedBaseValue = NormalizeValue(definition, defaultBaseValue);
            var node = new StatNode
            {
                Definition = definition,
                BaseValue = normalizedBaseValue,
                ComputedBaseValue = normalizedBaseValue,
                FinalValue = normalizedBaseValue,
                Dirty = false,
                IsDerived = false,
            };

            _stats.Add(id, node);
            SyncResourcesForProperty(id);
        }

        /// <summary>
        /// 注册一个派生属性，其基础值由依赖属性通过公式动态计算。
        /// </summary>
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

                if (!_stats.ContainsKey(dependency))
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
            var node = new StatNode
            {
                Definition = definition,
                Dirty = true,
                IsDerived = true,
                Dependencies = dependencyCopy,
                Formula = formula,
            };

            _stats.Add(id, node);

            for (int i = 0; i < dependencyCopy.Length; i++)
            {
                GetNode(dependencyCopy[i]).Dependents.Add(id);
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
        public bool IsRegistered(PropertyType id)
        {
            return _stats.ContainsKey(id);
        }

        /// <summary>
        /// 注册一个资源对象，并绑定其最大值来源属性。
        /// </summary>
        public ResourceValue RegisterPropertyBoundResource(ResourceType resourceType, PropertyType maxPropertyType)
        {
            ThrowIfResourceRegistered(resourceType);

            var resource = new ResourceValue();
            var node = new ResourceNode(resource, new PropertyBoundMaxPolicy(maxPropertyType));
            _resources.Add(resourceType, node);
            SyncResourceMaxValue(resourceType, true);
            return resource;
        }

        /// <summary>
        /// 注册一个不依赖属性的独立资源对象。
        /// </summary>
        public ResourceValue RegisterStandaloneResource(ResourceType resourceType, int maxValue, bool resetCurrent = true)
        {
            ThrowIfResourceRegistered(resourceType);

            var resource = new ResourceValue();
            var node = new ResourceNode(resource, new FixedMaxPolicy(maxValue));
            _resources.Add(resourceType, node);

            int resolvedMax = node.MaxPolicy.ResolveMaxValue(this);
            resource.SyncMaxValue(resolvedMax, resetCurrent);
            return resource;
        }

        /// <summary>
        /// 设置独立资源的最大值。若资源是属性绑定型则返回 false。
        /// </summary>
        public bool SetResourceMax(ResourceType resourceType, int maxValue, bool keepPercent = false)
        {
            if (!_resources.TryGetValue(resourceType, out var node))
            {
                return false;
            }

            if (!node.MaxPolicy.TrySetMaxValue(maxValue))
            {
                return false;
            }

            int oldMax = node.Resource.MaxValue;
            int oldValue = node.Resource.Value;

            int resolvedMax = node.MaxPolicy.ResolveMaxValue(this);
            node.Resource.SyncMaxValue(resolvedMax, false);

            if (!keepPercent)
            {
                return true;
            }

            int targetValue;
            if (oldMax <= 0)
            {
                targetValue = node.Resource.MaxValue;
            }
            else
            {
                double ratio = (double)oldValue / oldMax;
                targetValue = (int)Math.Round(ratio * node.Resource.MaxValue, MidpointRounding.AwayFromZero);
            }

            if (targetValue < 0)
            {
                targetValue = 0;
            }
            else if (targetValue > node.Resource.MaxValue)
            {
                targetValue = node.Resource.MaxValue;
            }

            int delta = targetValue - node.Resource.Value;
            if (delta > 0)
            {
                node.Resource.Add(delta);
            }
            else if (delta < 0)
            {
                node.Resource.Minus(-delta);
            }

            return true;
        }

        /// <summary>
        /// 尝试获取已注册的资源对象。
        /// </summary>
        public bool TryGetResource(ResourceType resourceType, out ResourceValue resource)
        {
            if (_resources.TryGetValue(resourceType, out var node))
            {
                resource = node.Resource;
                return true;
            }

            resource = null;
            return false;
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
        public void SetBaseValue(PropertyType id, int value)
        {
            var node = GetNode(id);
            if (node.IsDerived)
            {
                throw new InvalidOperationException("Cannot set base value on derived property: " + id);
            }

            value = NormalizeValue(node.Definition, value);

            if (node.BaseValue == value)
            {
                return;
            }

            node.BaseValue = value;
            node.ComputedBaseValue = value;
            MarkDirty(id);
        }

        /// <summary>
        /// 获取属性当前基础值（普通属性为 BaseValue，派生属性为公式计算值）。
        /// </summary>
        public int GetBaseValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetNode(id).ComputedBaseValue;
        }

        /// <summary>
        /// 获取属性最终值（基础值叠加所有激活 Modifier 后并做上下限裁剪）。
        /// </summary>
        public int GetFinalValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetNode(id).FinalValue;
        }

        /// <summary>
        /// 尝试获取属性最终值；若属性未注册则返回 false。
        /// </summary>
        public bool TryGetFinalValue(PropertyType id, out int value)
        {
            if (!_stats.ContainsKey(id))
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
        public ModifierHandle AddModifier(PropertyType id, int value, ModifierType type, ModifierSource source)
        {
            var node = GetNode(id);
            var modifier = new StatModifier
            {
                Handle = _nextHandle++,
                Value = value,
                Type = type,
                Source = source,
                Active = true,
            };

            node.Modifiers.Add(modifier);
            MarkDirty(id);
            return new ModifierHandle(modifier.Handle);
        }

        /// <summary>
        /// 更新指定 Modifier 的数值。
        /// </summary>
        public bool UpdateModifierValue(PropertyType id, ModifierHandle handle, int newValue)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var node = GetNode(id);
            for (int i = 0; i < node.Modifiers.Count; i++)
            {
                var modifier = node.Modifiers[i];
                if (modifier.Handle != handle.Value || modifier.Value == newValue)
                {
                    continue;
                }

                modifier.Value = newValue;
                node.Modifiers[i] = modifier;
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除指定句柄对应的 Modifier。
        /// </summary>
        public bool RemoveModifier(PropertyType id, ModifierHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var node = GetNode(id);
            for (int i = 0; i < node.Modifiers.Count; i++)
            {
                if (node.Modifiers[i].Handle != handle.Value)
                {
                    continue;
                }

                RemoveAtSwapBack(node.Modifiers, i);
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置指定 Modifier 的激活状态。
        /// </summary>
        public bool SetModifierActive(PropertyType id, ModifierHandle handle, bool isActive)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            var node = GetNode(id);
            for (int i = 0; i < node.Modifiers.Count; i++)
            {
                var modifier = node.Modifiers[i];
                if (modifier.Handle != handle.Value || modifier.Active == isActive)
                {
                    continue;
                }

                modifier.Active = isActive;
                node.Modifiers[i] = modifier;
                MarkDirty(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 按来源批量移除所有属性上的 Modifier。
        /// </summary>
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
                foreach (var pair in _stats)
                {
                    var propertyType = pair.Key;
                    var node = pair.Value;
                    bool removedFromCurrentNode = false;

                    for (int modifierIndex = node.Modifiers.Count - 1; modifierIndex >= 0; modifierIndex--)
                    {
                        if (node.Modifiers[modifierIndex].Source != source)
                        {
                            continue;
                        }

                        RemoveAtSwapBack(node.Modifiers, modifierIndex);
                        removedCount++;
                        removedFromCurrentNode = true;
                    }

                    if (removedFromCurrentNode)
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
        public void RecalculateIfDirty(PropertyType id)
        {
            var node = GetNode(id);
            if (!node.Dirty)
            {
                return;
            }

            if (!_recalculating.Add(id))
            {
                throw new InvalidOperationException("Circular derived property recalculation detected: " + id);
            }

            try
            {
                int oldValue = node.FinalValue;
                int baseValue = ResolveBaseValue(node);
                int finalValue = CalculateFinalValue(node.Definition, baseValue, node.Modifiers);

                node.ComputedBaseValue = baseValue;
                node.FinalValue = finalValue;
                node.Dirty = false;

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
            foreach (var propertyType in _stats.Keys)
            {
                RecalculateIfDirty(propertyType);
            }
        }

        private void ThrowIfPropertyRegistered(PropertyType id)
        {
            if (_stats.ContainsKey(id))
            {
                throw new InvalidOperationException("Property already registered: " + id);
            }
        }

        private void ThrowIfResourceRegistered(ResourceType resourceType)
        {
            if (_resources.ContainsKey(resourceType))
            {
                throw new InvalidOperationException("Resource already registered: " + resourceType);
            }
        }

        private void InitializeDerivedProperty(PropertyType id)
        {
            var node = GetNode(id);
            int baseValue = ResolveBaseValue(node);
            int finalValue = CalculateFinalValue(node.Definition, baseValue, node.Modifiers);

            node.ComputedBaseValue = baseValue;
            node.FinalValue = finalValue;
            node.Dirty = false;
            SyncResourcesForProperty(id);
        }

        private int ResolveBaseValue(StatNode node)
        {
            if (!node.IsDerived)
            {
                return node.BaseValue;
            }

            for (int i = 0; i < node.Dependencies.Length; i++)
            {
                RecalculateIfDirty(node.Dependencies[i]);
            }

            return node.Formula(new DerivedPropertyContext(this));
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

            var node = GetNode(id);
            node.Dirty = true;

            for (int i = 0; i < node.Dependents.Count; i++)
            {
                MarkDirtyRecursive(node.Dependents[i], visited);
            }
        }

        private void RecalculateDirtyCascade(PropertyType id, HashSet<PropertyType> visited)
        {
            if (!visited.Add(id))
            {
                return;
            }

            RecalculateIfDirty(id);

            var node = GetNode(id);
            for (int i = 0; i < node.Dependents.Count; i++)
            {
                RecalculateDirtyCascade(node.Dependents[i], visited);
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

            var node = GetNode(current);
            if (!node.IsDerived)
            {
                return false;
            }

            for (int i = 0; i < node.Dependencies.Length; i++)
            {
                if (HasDependencyPath(node.Dependencies[i], target, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private StatNode GetNode(PropertyType id)
        {
            if (_stats.TryGetValue(id, out var node))
            {
                return node;
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
            foreach (var pair in _resources)
            {
                if (!pair.Value.MaxPolicy.TryGetBoundProperty(out var boundPropertyType))
                {
                    continue;
                }

                if (boundPropertyType != propertyType)
                {
                    continue;
                }

                SyncResourceMaxValue(pair.Key, false);
            }
        }

        private void SyncResourceMaxValue(ResourceType resourceType, bool forceResetWhenUninitialized)
        {
            if (!_resources.TryGetValue(resourceType, out var node))
            {
                return;
            }

            var resource = node.Resource;
            int maxValue = node.MaxPolicy.ResolveMaxValue(this);
            bool resetCurrent = forceResetWhenUninitialized && resource.MaxValue == 0 && resource.Value == 0;
            resource.SyncMaxValue(maxValue, resetCurrent);
        }
    }
}
