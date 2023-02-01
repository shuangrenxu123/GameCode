using System;
using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    public HealthPoint hp = new HealthPoint();
    public ActionPointManager ActionPointManager = new ActionPointManager();
    public CombatNumberBox numberBox = new CombatNumberBox();
    public void Init(int h)
    {
        hp.SetMaxValue(h);
        ActionPointManager.Init();
        hp.Reset();
    }
    public void AddListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        ActionPointManager.AddListener(actionPointType, action);
    }

    public void RemoveListener(ActionPointType actionPointType, Action<CombatAction> action)
    {
        ActionPointManager.RemoveListener(actionPointType, action);
    }

    public void TriggerActionPoint(ActionPointType actionPointType, CombatAction action)
    {
        ActionPointManager.TriggerActionPoint(actionPointType, action);
    }
}
