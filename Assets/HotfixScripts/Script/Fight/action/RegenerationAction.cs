using Fight;
using System.Collections.Generic;

/// <summary>
/// 恢复
/// </summary>
public class RegenerationAction : CombatAction
{
    int result;
    public RegenerationAction(CombatEntity creater, List<CombatEntity> targets)
    {
        Creator = creater;
        Target = targets;
    }
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
            target.ActionPointManager.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
    }

    protected override void PreProcess(CombatEntity c, CombatEntity t)
    {
        foreach (var target in Target)
        {
            target.ActionPointManager.TriggerActionPoint(ActionPointType.PreRestoreHP, this);
        }
    }
}
