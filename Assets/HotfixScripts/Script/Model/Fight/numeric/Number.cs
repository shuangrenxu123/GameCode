using System;
using System.Collections.Generic;
using System.Linq;

namespace Fight.Number
{
    public enum ModifierType
    {
        Flat,   // 普通加值
        Percent // 百分比加值
    }

    public class Modifier
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int BaseValue { get; } // 原始值
        public int CurrentValue { get; set; } // 当前值（应用增强后）
        public ModifierType Type { get; }
        public bool IsActive { get; set; } = true;
        public PropertySourceTypes Source { get; } // 修改来源（如"装备"、"Buff"）

        public Modifier(int value, ModifierType type, PropertySourceTypes source)
        {
            BaseValue = value;
            CurrentValue = value;
            Type = type;
            Source = source;
        }
    }

    public class PropertyValue
    {
        private int _baseValue;
        public int finalValue { get; private set; }
        private readonly Dictionary<Guid, Modifier> _modifiers = new();


        public event Action<int> OnValueChanged; // 数值变化事件

        public PropertyValue(int baseValue)
        {
            _baseValue = baseValue;
            finalValue = _baseValue;
        }

        public int BaseValue
        {
            get => _baseValue;
        }

        public Guid AddModifier(int value, ModifierType type, PropertySourceTypes source)
        {
            var modifier = new Modifier(value, type, source);

            _modifiers.Add(modifier.Id, modifier);
            NotifyValueChanged();
            return modifier.Id;
        }

        public bool RemoveModifier(Guid id)
        {
            if (_modifiers.Remove(id, out var _))
            {
                NotifyValueChanged();
                return true;
            }
            return false;
        }

        public bool SetModifierActive(Guid id, bool isActive)
        {
            if (_modifiers.TryGetValue(id, out var modifier) && modifier.IsActive != isActive)
            {
                modifier.IsActive = isActive;
                NotifyValueChanged();
                return true;
            }
            return false;
        }

        public void RemoveModifiersFromSource(PropertySourceTypes source)
        {

            bool remove = false;
            var list = _modifiers.Where(x => x.Value.Source == source);
            foreach (var modifier in list)
            {
                _modifiers.Remove(modifier.Value.Id);
            }

            if (remove)
                NotifyValueChanged();
        }

        public int CalculateValue()
        {
            int flatSum = 0;
            int percentSum = 0;

            foreach (var modifier in _modifiers.Values)
            {
                if (!modifier.IsActive) continue;

                switch (modifier.Type)
                {
                    case ModifierType.Flat:
                        flatSum += modifier.CurrentValue;
                        break;
                    case ModifierType.Percent:
                        percentSum += modifier.CurrentValue;
                        break;
                }
            }
            finalValue = (int)((_baseValue + flatSum) * (1f + percentSum / 100f));
            return finalValue;
        }

        private void NotifyValueChanged()
        {
            OnValueChanged?.Invoke(CalculateValue());
        }
    }

    public enum PropertySourceTypes
    {
        Self,
        Equipment,
        Buff,
    }
}
