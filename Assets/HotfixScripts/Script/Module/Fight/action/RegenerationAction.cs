using System.Collections.Generic;
using Fight;

/// <summary>
/// 恢复
/// </summary>
public class RegenerationAction : CombatAction
{
    int result;
    public override void Apply(int baseValue)
    {
        foreach (var target in Target)
        {
            result = baseValue;
            PreProcess(Creator, target);
            target.hp.Add(result);
            PostProcess(Creator, target);
        }
    }

    protected override void PostProcess(CombatEntity c, CombatEntity t)
    {
        foreach (var target in Target)
        {
            target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
    }

    protected override void PreProcess(CombatEntity c, CombatEntity t)
    {
        foreach (var target in Target)
        {
            target.TriggerActionPoint(ActionPointType.PreRestoreHP, this);
        }
    }
}
