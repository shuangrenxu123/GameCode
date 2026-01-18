using System.Collections.Generic;
using Fight;
using UnityEngine;

public class HemophagiaBuff : BuffBase
{
    public HemophagiaBuff(BuffManager manager, CombatEntity c) : base(manager, c)
    {
        data = UnityEngine.Resources.Load<BuffDataBase>("buff/hemophagia");
    }
    public override void OnAdd()
    {
        BuffManager.entity.AddListener(ActionPointType.PostCauseDamage, blood);
        Debug.Log("获得了吸血buff");
    }

    private void blood(CombatAction obj)
    {
        var o = obj as DamageAction;
        if (o != null)
        {
            CombatActionFactor.CreateActionAndExecute<RegenerationAction>
                (o.Creator, new List<CombatEntity> { o.Creator }, o.damage / 2);
        }
    }
    public override void OnDestroy()
    {
        BuffManager.entity.RemoveListener(ActionPointType.PostCauseDamage, blood);
    }
}
