using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ��ֵ���ϣ��ɵײ���ֵ��϶���
/// </summary>
public class IntCollector : Number
{
    /// <summary>
    /// buff���ṩ���ܺ�
    /// </summary>
    public int BuffValue { get; private set; }
    /// <summary>
    /// �����ṩ����ֵ
    /// </summary>
    public int BaseValue { get; private set; }
    /// <summary>
    /// װ���ṩ��ֵ
    /// </summary>
    public int EquipeValue { get; private set; }
    /// <summary>
    /// ����ֵ
    /// </summary>
    public int TotalValue { get; private set; }
    public new int Value => TotalValue;
    Dictionary<PropertySourceType, List<Number>> propertySource;
    public IntCollector(PropertySourceType type)
    {
        this.Type = type;
        propertySource = new Dictionary<PropertySourceType, List<Number>>();
    }
    public void AddInt(Number value)
    {
        if (propertySource.ContainsKey(value.Type))
            propertySource[value.Type].Add(value);
        else
        {
            List<Number> list = new List<Number>
            {
                value
            };
            propertySource.Add(value.Type, list);
        }
        UpdateValue();
    }
    public void RemoveInt(Number value) 
    {
        if (propertySource.ContainsKey(value.Type))
            propertySource[value.Type].Remove(value);
        UpdateValue();
    }
    private void UpdateValue()
    {
        if (propertySource.ContainsKey(PropertySourceType.Buff))
            BuffValue = propertySource[PropertySourceType.Buff].Sum(x => x.Value);
        if (propertySource.ContainsKey(PropertySourceType.Equipe))
            EquipeValue =  propertySource[PropertySourceType.Equipe].Sum(x => x.Value);
        if (propertySource.ContainsKey(PropertySourceType.Self))
            BaseValue =  propertySource[PropertySourceType.Self].Sum(x => x.Value);
        TotalValue = BuffValue + EquipeValue + BaseValue;
}
}
