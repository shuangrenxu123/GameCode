using TMPro;
using UnityEngine;
namespace Fight
{
    public class DamageAction : CombatAction
    {
        public int damage;
        public string animator;
        public DamageAction(CombatEntity creater, CombatEntity[] targets)
        {
            Creator = creater;
            Target = targets;
        }
        public override void Apply(int baseValue)
        {
            foreach (var target in Target)
            {
                damage = baseValue;
                damage = Creator.numberBox.Atk.Value;
                PreProcess(Creator,target);
                target.TakeDamage(damage,animator);
                PostProcess(Creator, target);
            }
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
            foreach(var target in Target)
            {
                target.ActionPointManager.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            }
            Debug.Log("------------触发了前置行为(如计算免伤等等)---------");
        }
    }
}