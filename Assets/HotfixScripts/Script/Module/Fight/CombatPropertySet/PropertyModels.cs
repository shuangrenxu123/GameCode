using System;
using System.Collections.Generic;

namespace Fight.Number
{
    public readonly struct StatDefinition
    {
        public readonly int DefaultBaseValue;
        public readonly int MinFinalValue;
        public readonly int MaxFinalValue;

        public StatDefinition(int defaultBaseValue, int minFinalValue, int maxFinalValue)
        {
            DefaultBaseValue = defaultBaseValue;
            MinFinalValue = minFinalValue;
            MaxFinalValue = maxFinalValue;
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

    internal sealed class StatNode
    {
        public StatDefinition Definition;
        public int BaseValue;
        public int ComputedBaseValue;
        public int FinalValue;
        public bool Dirty;
        public bool IsDerived;
        public PropertyType[] Dependencies = Array.Empty<PropertyType>();
        public readonly List<PropertyType> Dependents = new List<PropertyType>(2);
        public Func<DerivedPropertyContext, int> Formula;
        public readonly List<StatModifier> Modifiers = new List<StatModifier>(4);
    }

    internal interface IResourceMaxPolicy
    {
        int ResolveMaxValue(CombatPropertySet owner);
        bool TryGetBoundProperty(out PropertyType propertyType);
        bool TrySetMaxValue(int maxValue);
    }

    internal sealed class PropertyBoundMaxPolicy : IResourceMaxPolicy
    {
        private readonly PropertyType _maxPropertyType;

        public PropertyBoundMaxPolicy(PropertyType maxPropertyType)
        {
            _maxPropertyType = maxPropertyType;
        }

        public int ResolveMaxValue(CombatPropertySet owner)
        {
            return owner.TryGetFinalValue(_maxPropertyType, out var maxValue) ? maxValue : 0;
        }

        public bool TryGetBoundProperty(out PropertyType propertyType)
        {
            propertyType = _maxPropertyType;
            return true;
        }

        public bool TrySetMaxValue(int maxValue)
        {
            return false;
        }
    }

    internal sealed class FixedMaxPolicy : IResourceMaxPolicy
    {
        private int _maxValue;

        public FixedMaxPolicy(int maxValue)
        {
            _maxValue = maxValue;
        }

        public int ResolveMaxValue(CombatPropertySet owner)
        {
            return _maxValue;
        }

        public bool TryGetBoundProperty(out PropertyType propertyType)
        {
            propertyType = default;
            return false;
        }

        public bool TrySetMaxValue(int maxValue)
        {
            _maxValue = maxValue;
            return true;
        }
    }

    internal sealed class ResourceNode
    {
        public readonly ResourceValue Resource;
        public readonly IResourceMaxPolicy MaxPolicy;

        public ResourceNode(ResourceValue resource, IResourceMaxPolicy maxPolicy)
        {
            Resource = resource;
            MaxPolicy = maxPolicy;
        }
    }
}
