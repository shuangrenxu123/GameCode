using UnityEngine;
/// <summary>
/// 血条组件
/// </summary>
public class HealthPoint
{
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
    /// <param name="value"></param>
    public void Minus(int value)
    {
        Value = Mathf.Max(0, Value - value);
    }
    /// <summary>
    /// 加血
    /// </summary>
    /// <param name="value"></param>
    public void Add(int value)
    {
        Value = Mathf.Min(MaxValue, Value + value);
    }
    /// <summary>
    /// 返回当前生命百分比
    /// </summary>
    /// <returns></returns>
    public float Percent()
    {
        return (float)Value / MaxValue;
    }
}
