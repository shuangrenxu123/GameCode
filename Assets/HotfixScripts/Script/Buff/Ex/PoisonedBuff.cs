using Fight;
using UnityEngine;

public class PoisonedBuff : BuffBase
{
    float deltaTime = 0.5f;
    int count = 1;
    public PoisonedBuff(BuffManager manager, CombatEntity c) : base(manager, c)
    {
        data = UnityEngine.Resources.Load<BuffDataBase>("buff/poisoned");
    }
    public override void OnAdd()
    {
        Debug.Log("获得了中毒buff");
    }
    public override void OnTrigger()
    {
        if (nowtime >= deltaTime * count)
        {
            new DamageAction(BuffManager.entity, new CombatEntity[] { BuffManager.entity }).Apply(10);
            count += 1;
        }
    }
    public override void OnDestory()
    {
        Debug.Log("中毒buff移除");
    }
}
