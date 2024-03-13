using Fight;
using UnityEngine;
using Utilities;
[CreateAssetMenu(menuName = "Items/consumableItem")]
// 消耗品
public class ConsumableItemData : ItemData
{
    public override ItemType Type => ItemType.Consumable;
    [Header("相关信息")]
    public int MaxItemAmount;
    [Header("Animtions")]
    public string consumeAnimation;
    public string UsageFailedAnimation;
    [Range(0f, 1f)]
    public float effectTime;
    public bool isInteracting;
    public GameObject modelPrefab;
    /// <summary>
    /// 消耗品所更改的数值
    /// </summary>
    public ChangeType changeType;
    [Condition("changeType",ConditionAttribute.ConditionType.IsLessThan,ConditionAttribute.VisibilityType.Hidden,5)]
    public int value;

    public void Effect(CombatEntity me)
    {        
        switch (changeType)
        {
            case ChangeType.HP:
                if (value >= 0)
                {
                    var action = new RegenerationAction(me, new CombatEntity[] { me });
                    action.Apply(value);
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

public enum ChangeType
{
    HP=1,
    MP=2,
    Atk=3,
    Def=4,
    Other=5,
}
