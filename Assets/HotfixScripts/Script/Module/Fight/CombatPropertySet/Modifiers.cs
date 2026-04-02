using System;

namespace Fight.Number
{
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

    internal struct StatModifier
    {
        public int Handle;
        public int Value;
        public ModifierType Type;
        public ModifierSource Source;
        public bool Active;
    }
}
