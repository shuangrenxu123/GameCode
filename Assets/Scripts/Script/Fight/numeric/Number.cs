public class Number
{
    /// <summary>
    /// ����ֵ
    /// </summary>
    public float baseValue = 0;
    /// <summary>
    /// װ���ӳ�ֵ
    /// </summary>
    public float equipValue = 0;
    /// <summary>
    /// buff�ӳ�ֵ
    /// </summary>
    public float buffValue = 0;
    /// <summary>
    /// ����ֵ
    /// </summary>
    public float Value = 0;//����ֵ


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
