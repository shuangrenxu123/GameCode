public class Number
{
    /// <summary>
    /// 基础值
    /// </summary>
    public int baseValue = 0;
    /// <summary>
    /// 装备加成值
    /// </summary>
    public int equipValue = 0;
    /// <summary>
    /// buff加成值
    /// </summary>
    public int buffValue = 0;
    /// <summary>
    /// 最终值
    /// </summary>
    public int Value = 0;//最终值


    public void SetBase(int value)
    {
        baseValue = value;
        UpdateValue();
    }

    public void UpdateValue()
    {
        Value = baseValue + equipValue + buffValue;
    }
}
