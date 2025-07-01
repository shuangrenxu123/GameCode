using System;
using System.Collections.Generic;
using Fight.Number;

namespace Fight.Number
{
    public class CombatNumberBox
    {
        public enum PropertyType
        {
            Attack,
            Defense,
            Speed,

        }
        private readonly Dictionary<PropertyType, PropertyValue> properties = new();

        public void Init()
        {
            // 注册属性
            RegisterAttribute(PropertyType.Attack, 100);
            RegisterAttribute(PropertyType.Defense, 50);
        }


        public void RegisterAttribute(PropertyType type, int baseValue)
        {
            if (!properties.ContainsKey(type))
            {
                properties[type] = new PropertyValue(baseValue);
            }
        }

        public int GetPropertyValue(PropertyType type)
        {
            if (properties.TryGetValue(type, out var property))
            {
                return property.finalValue;
            }
            throw new Exception("不存在该属性");
        }

        public Guid AddModifier(PropertyType type, int value, ModifierType modifierType, PropertySourceTypes source)
        {
            var property = GetProperty(type);

            var modifier = property.AddModifier(value, modifierType, source);

            return modifier;
        }

        public void RemoveModifier(PropertyType type, Guid modifier)
        {
            var property = GetProperty(type);
            property.RemoveModifier(modifier);
        }

        private PropertyValue GetProperty(PropertyType type)
        {
            if (properties.TryGetValue(type, out var p))
            {
                return p;
            }
            throw new Exception($"no PropertyType {type}");
        }
    }
}
