using System;
using UnityEngine;

namespace Fight.Number
{

    /// <summary>
    /// 血条组件
    /// </summary>
    public class HealthPoint
    {
        public event Action<int, int> OnHPChange;
        public event Action<int, int> OnHpAdd;
        public event Action<int, int> OnHpMinus;
        public event Action OnHit;
        public event Action OnDead;

        public float Percent
        {
            get
            {
                if (MaxValue == 0)
                {
                    return 1;
                }
                return (float)Value / MaxValue;
            }
        }

        public int Value { get; private set; }
        public int MaxValue { get; private set; }

        public void Reset()
        {
            Value = MaxValue;
        }
        public void SetMaxValue(int value)
        {
            MaxValue = value;
            Reset();
        }

        /// <summary>
        /// 扣血
        /// </summary>
        /// <param name="minusValue"></param>
        public void Minus(int minusValue)
        {
            var oldValue = Value;
            Value = Mathf.Max(0, Value - minusValue);
            OnHPChange?.Invoke(oldValue, Value);
            OnHpMinus?.Invoke(oldValue, Value);
            OnHit?.Invoke();
        }

        /// <summary>
        /// 加血
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            var oldValue = Value;
            Value = Mathf.Min(MaxValue, Value + value);
            OnHpAdd?.Invoke(oldValue, Value);
            OnHPChange?.Invoke(oldValue, Value);
        }
    }
}