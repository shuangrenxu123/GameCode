using UnityEngine;
namespace Fight
{
    public class DamageAction : CombatAction
    {
        public IntCollector damage;
        public DamageAction(CombatEntity creater, CombatEntity[] targets)
        {
            Creator = creater;
            Target = targets;
            damage = new IntCollector(PropertySourceType.Self);
        }
        public override void Apply(int baseValue)
        {
            //int damage = baseValue;
            damage.AddInt(new IntNumber(baseValue,PropertySourceType.Self));
            foreach (var target in Target)
            {
                PreProcess(Creator, target);
                Process(Creator,target);
                PostProcess(Creator, target);
            }
        }

        public void Process(CombatEntity c, CombatEntity t)
        {
            t.hp.Minus(damage.Value);
        }
        /// <summary>
        /// 后置行为
        /// </summary>
        protected override void PostProcess(CombatEntity c, CombatEntity t)
        {
            Debug.Log("-------------触发了后置行为(如吸血等)-----------");
            Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
            foreach (var target in Target)
            {
                target.ActionPointManager.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
            }
        }
        /// <summary>
        /// 前置行为
        /// </summary>
        protected override void PreProcess(CombatEntity c, CombatEntity t)
        {
            Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
            foreach (var target in Target)
            {
                target.ActionPointManager.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            }
            Debug.Log("------------触发了前置行为(如计算免伤等等)---------");
        }
    }
}