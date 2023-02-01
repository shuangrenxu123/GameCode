using TMPro;
using UnityEngine;

public class DamageAction : CombatAction
{
    int damage = 10;
    public DamageAction(CombatEntity creater, CombatEntity target)
    {
        Creator = creater;
        Target = target;
    }
    public override void Allpy()
    {
        PreProcess();
        Target.hp.Minus(damage);
        GameObject go = PoolManager.Instance.GetGameObjectToPool("hppanel", Target.gameObject.transform.position + new Vector3(0, 0.5f, 0),
            Quaternion.Euler(0, 180, 0));
        go.GetComponent<TextMeshPro>().text = damage.ToString();
        PostProcess();
    }
    /// <summary>
    /// ������Ϊ
    /// </summary>
    public override void PostProcess()
    {
        Debug.Log("-------------�����˺�����Ϊ(����Ѫ��)-----------");
        Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
        Target.ActionPointManager.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
    }
    /// <summary>
    /// ǰ����Ϊ
    /// </summary>
    public override void PreProcess()
    {
        Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
        Target.ActionPointManager.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
        Debug.Log("------------������ǰ����Ϊ(��������˵ȵ�)---------");
    }
}
