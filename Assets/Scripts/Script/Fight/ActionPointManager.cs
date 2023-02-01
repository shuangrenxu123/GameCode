using System;
using System.Collections.Generic;

public class ActionPointManager
{
    Dictionary<ActionPointType, ActionPoint> ActionPoints = new Dictionary<ActionPointType, ActionPoint>();
    /// <summary>
    /// ��ʼ����Ϊ�㡣ע��ĳ����Ϊ��
    /// </summary>
    public void Init()
    {
        ActionPoints.Add(ActionPointType.PreReceiveDamage, new ActionPoint());
        ActionPoints.Add(ActionPointType.PostCauseDamage, new ActionPoint());
    }
    public void AddListener(ActionPointType type, Action<CombatAction> action)
    {
        ActionPoints[type].Listeners.Add(action);
    }
    public void RemoveListener(ActionPointType type, Action<CombatAction> action)
    {
        ActionPoints[type].Listeners.Remove(action);
    }
    public void TriggerActionPoint(ActionPointType type, CombatAction action)
    {
        if (ActionPoints.ContainsKey(type))
        {
            foreach (var item in ActionPoints[type].Listeners)
            {
                item.Invoke(action);
            }
        }
    }
}
