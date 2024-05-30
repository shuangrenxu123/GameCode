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
        BuffManager.entity.ActionPointManager.AddListener(ActionPointType.PostCauseDamage, blood);
        Debug.Log("获得了吸血buff");
    }

    private void blood(CombatAction obj)
    {
        var o = obj as DamageAction;
        if (o != null)
        {
            new RegenerationAction(o.Creator, new CombatEntity[] { o.Creator }).Apply(((int)o.damage.Value / 2));
        }
    }
    public override void OnDestory()
    {
        BuffManager.entity.ActionPointManager.RemoveListener(ActionPointType.PostCauseDamage, blood);
    }
}
