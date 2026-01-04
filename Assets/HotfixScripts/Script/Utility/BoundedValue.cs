
using System;

namespace UtilityNumber
{
    public class RangeValue_Int
    {
        public int value { get; protected set; }
        public int minValue { get; protected set; }
        public int maxValue { get; protected set; }

        public RangeValue_Int(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            SetValue(minValue);
        }

        public void SetValue(int newValue)
        {
            value = Math.Min(Math.Max(newValue, minValue), maxValue);
        }

        public void Add(int amount)
        {
            SetValue(value + amount);
        }

        public void Subtract(int amount)
        {
            SetValue(value - amount);
        }
    }

    public class RangeValue_Float
    {
        public float value { get; protected set; }
        public float minValue { get; protected set; }
        public float maxValue { get; protected set; }

        public RangeValue_Float(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            SetValue(minValue);
        }

        public void SetValue(float newValue)
        {
            value = Math.Min(Math.Max(newValue, minValue), maxValue);
        }
        public void Add(float amount)
        {
            SetValue(value + amount);
        }
        public void Subtract(float amount)
        {
            SetValue(value - amount);
        }
    }

    /// <summary>
    /// 一个用于有最大值
    /// </summary>
    public class BoundedValue_Int : RangeValue_Int
    {
        public event Action<int, int> OnValueChange;
        public event Action<int, int> OnValueAdd;
        public event Action<int, int> OnValueSubtract;
        public float Percent
        {
            get
            {
                if (maxValue == 0)
                {
                    return 1;
                }
                return (float)value / maxValue;
            }
        }
        public bool isPositive { get; private set; }


        public BoundedValue_Int(int maxValue, bool isPositive = true) : base(0, maxValue)
        {
            this.isPositive = isPositive;
            this.maxValue = maxValue;
            SetValue(isPositive ? 0 : maxValue);
        }

        public void Reset()
        {
            value = maxValue;
        }
    }

    public class BoundedValue_Float : RangeValue_Float
    {

        public event Action<float, float> OnValueChange;
        public event Action<float, float> OnValueAdd;
        public event Action<float, float> OnValueSubtract;

        public bool isPositive { get; private set; }

        public BoundedValue_Float(float maxValue, bool isPositive = true) : base(0, maxValue)
        {
            this.maxValue = maxValue;
            this.isPositive = isPositive;
            SetValue(isPositive ? 0 : maxValue);
        }

        public void Reset()
        {
            value = maxValue;
        }
    }

}