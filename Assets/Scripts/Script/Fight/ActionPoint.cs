using System;
using System.Collections.Generic;

public class ActionPoint
{
    public List<Action<CombatAction>> Listeners = new List<Action<CombatAction>>();
}
public enum ActionPointType
{
    /// <summary>
    /// ����˺�ǰ
    /// </summary>
    PreCauseDamage,
    /// <summary>
    /// �����˺�ǰ
    /// </summary>
    PreReceiveDamage,
    /// <summary>
    /// ����˺���
    /// </summary>
    PostCauseDamage,
    /// <summary>
    /// �����˺���
    /// </summary>
    PostReceiveDamage,
}
