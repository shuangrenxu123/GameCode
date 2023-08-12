using UnityEngine;
/// <summary>
/// 血条组件
/// </summary>
public class HealthPoint
{
    private StateUI stateui;
    public int Value { get; private set; }
    public int MaxValue { get; private set; }
    public void Init(bool isPlayer)
    {
        if (isPlayer)
        {
            stateui = WindowsManager.Instance.GetUiWindow<StateUI>();
        }
        else
        {
            stateui = WindowsManager.Instance.GetUiWindow<EnemyStateUI>();
        }
    }
    public void Reset()
    {
        Value = MaxValue;
        UpdateHPUI();
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
        UpdateHPUI();
    }
    /// <summary>
    /// 加血
    /// </summary>
    /// <param name="value"></param>
    public void Add(int value)
    {
        Value = Mathf.Min(MaxValue, Value + value);
        UpdateHPUI();
    }
    /// <summary>
    /// 返回当前生命百分比
    /// </summary>
    /// <returns></returns>
    public float Percent()
    {
        return (float)Value / MaxValue;
    }
    private void UpdateHPUI()
    {
        stateui.SetHPPercent(Percent());
    }
}
