using System;
using System.Collections.Generic;

namespace Fight.Number
{
    /// <summary>
    /// Modifier 的整数缩放规则：
    /// 1. Add：直接加减绝对值，例如 +10 攻击。
    /// 2. AddPercent：按百分数叠加，100 表示 100%，15 表示 15%。
    /// 3. Multiply：按倍率百分数相乘，100 表示 x1.0，150 表示 x1.5，50 表示 x0.5。
    /// 4. Override：直接覆盖最终值，不参与其他计算。
    ///
    /// 属性本身如果是倍率/百分比语义，也统一使用相同规则：
    /// 100 表示 100%，业务层读取后自行 / 100f 转成浮点倍率。
    ///
    /// 当前系统统一只处理 int。
    /// 如果未来需要更细的小数精度，必须在“该属性的生产方、Modifier、消费方”三处使用同一套放大规则。
    /// </summary>
    public enum ModifierType
    {
        Add = 0,
        AddPercent = 1,
        Multiply = 2,
        Override = 3,
    }

    public enum ModifierSourceType
    {
        None = 0,
        Self = 1,
        Equipment = 2,
        Buff = 3,
        Skill = 4,
        Talent = 5,
    }

    public readonly struct ModifierSource : IEquatable<ModifierSource>
    {
        public readonly ModifierSourceType Type;
        public readonly int SourceKey;

        public ModifierSource(ModifierSourceType type, int sourceKey = 0)
        {
            Type = type;
            SourceKey = sourceKey;
        }

        public bool Equals(ModifierSource other)
        {
            return Type == other.Type && SourceKey == other.SourceKey;
        }

        public override bool Equals(object obj)
        {
            return obj is ModifierSource other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, SourceKey);
        }

        public static bool operator ==(ModifierSource left, ModifierSource right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModifierSource left, ModifierSource right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct ModifierHandle : IEquatable<ModifierHandle>
    {
        public static readonly ModifierHandle Invalid = new ModifierHandle(0);

        public readonly int Value;

        public ModifierHandle(int value)
        {
            Value = value;
        }

        public bool IsValid
        {
            get { return Value > 0; }
        }

        public bool Equals(ModifierHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ModifierHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(ModifierHandle left, ModifierHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModifierHandle left, ModifierHandle right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct DerivedPropertyContext
    {
        private readonly CombatPropertySet _owner;

        internal DerivedPropertyContext(CombatPropertySet owner)
        {
            _owner = owner;
        }

        public int GetFinalValue(PropertyType id)
        {
            return _owner.GetFinalValue(id);
        }

        public bool TryGetFinalValue(PropertyType id, out int value)
        {
            return _owner.TryGetFinalValue(id, out value);
        }
    }

    internal struct StatModifier
    {
        public int Handle;
        public int Value;
        public ModifierType Type;
        public ModifierSource Source;
        public bool Active;
    }

    internal sealed class StatSlot
    {
        public int BaseValue;
        public int ComputedBaseValue;
        public int FinalValue;
        public bool Dirty;
        public bool IsDerived;
        public PropertyType[] Dependencies = Array.Empty<PropertyType>();
        public Func<DerivedPropertyContext, int> Formula;
        public readonly List<StatModifier> Modifiers = new List<StatModifier>(4);
    }

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

        public bool IsRegistered(PropertyType id)
        {
            return _definitions.ContainsKey(id);
        }

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

        public bool TryGetResource(ResourceType resourceType, out ResourceValue resource)
        {
            return _resources.TryGetValue(resourceType, out resource);
        }

        public void BeginBatch()
        {
            _batchDepth++;
        }

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

        public int GetBaseValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetSlot(id).ComputedBaseValue;
        }

        public int GetFinalValue(PropertyType id)
        {
            RecalculateIfDirty(id);
            return GetSlot(id).FinalValue;
        }

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
