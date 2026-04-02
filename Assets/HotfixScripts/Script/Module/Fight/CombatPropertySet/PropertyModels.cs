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
}
