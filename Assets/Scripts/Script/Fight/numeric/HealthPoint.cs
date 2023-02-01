using UnityEngine;
/// <summary>
/// Ѫ�����
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
    /// ��Ѫ
    /// </summary>
    /// <param name="value"></param>
    public void Minus(int value)
    {
        Value = Mathf.Max(0, Value - value);
    }
    /// <summary>
    /// ��Ѫ
    /// </summary>
    /// <param name="value"></param>
    public void Add(int value)
    {
        Value = Mathf.Min(MaxValue, Value + value);
    }
    /// <summary>
    /// ���ص�ǰ�����ٷֱ�
    /// </summary>
    /// <returns></returns>
    public float Percent()
    {
        return (float)Value / MaxValue;
    }
}
