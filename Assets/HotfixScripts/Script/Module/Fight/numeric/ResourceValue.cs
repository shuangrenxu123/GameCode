using System;

namespace Fight.Number
{
    public enum ResourceType
    {
        Hp = 0,
    }

    /// <summary>
    /// 资源层对象，负责管理“当前值 / 最大值 / 百分比 / 变化事件”。
    /// 当前资源最大值通常由 CombatPropertySet 中某个属性驱动，例如 HP 绑定到 MaxHp。
    /// </summary>
    public class ResourceValue
    {
        public event Action<int, int> OnValueChange;
        public event Action<int, int> OnValueAdd;
        public event Action<int, int> OnValueMinus;
        public event Action OnValueReduced;
        public event Action OnEmpty;

        public float Percent
        {
            get
            {
                if (MaxValue == 0)
                {
                    return 1f;
                }

                return (float)Value / MaxValue;
            }
        }

        public int Value { get; private set; }
        public int MaxValue { get; private set; }

        public void Reset()
        {
            SetCurrentInternal(MaxValue);
        }

        public void Add(int value)
        {
            SetCurrentInternal(Value + value);
        }

        public void Minus(int value)
        {
            SetCurrentInternal(Value - value);
        }

        internal void SyncMaxValue(int value, bool resetCurrent)
        {
            if (value < 0)
            {
                value = 0;
            }

            MaxValue = value;

            if (resetCurrent)
            {
                SetCurrentInternal(MaxValue);
                return;
            }

            if (Value > MaxValue)
            {
                SetCurrentInternal(MaxValue);
            }
        }

        private void SetCurrentInternal(int newValue)
        {
            if (newValue < 0)
            {
                newValue = 0;
            }
            else if (newValue > MaxValue)
            {
                newValue = MaxValue;
            }

            int oldValue = Value;
            if (oldValue == newValue)
            {
                return;
            }

            Value = newValue;
            OnValueChange?.Invoke(oldValue, Value);

            if (Value > oldValue)
            {
                OnValueAdd?.Invoke(oldValue, Value);
            }
            else
            {
                OnValueMinus?.Invoke(oldValue, Value);
                OnValueReduced?.Invoke();
            }

            if (Value <= 0)
            {
                OnEmpty?.Invoke();
            }
        }
    }
}
