using Fight;

/// <summary>
/// 恢复
/// </summary>
public class RegenerationAction : CombatAction
{
    int result;
    public RegenerationAction(CombatEntity creater, CombatEntity[] targets)
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
        
    }

    protected override void PreProcess(CombatEntity c, CombatEntity t)
    {

    }
}
