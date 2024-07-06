using Fight;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 属性可消耗品
/// </summary>
public class ConsumableItem
{
    public ConsumableItemData data;
    public GameObject go;
    public float effectTime => data.effectTime;
    public ConsumableItem(ConsumableItemData data)
    {
        this.data = data;
    }
    public void Effect(CombatEntity me)
    {
        switch (data.changeType)
        {
            case ChangeType.HP:
                if (data.value >= 0)
                {
                    var action = new RegenerationAction(me, new List<CombatEntity> { me });
                    action.Apply(data.value);
                }
                break;
            case ChangeType.MP:
                break;
            case ChangeType.Atk:
                break;
            case ChangeType.Def:
                break;
            case ChangeType.Other:
                break;

        }
    }


}
