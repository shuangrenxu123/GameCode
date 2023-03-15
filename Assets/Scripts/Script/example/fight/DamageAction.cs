using TMPro;
using UnityEngine;
namespace Fight
{
    public class DamageAction : CombatAction
    {
        int damage = 10;
        public DamageAction(CombatEntity creater, CombatEntity target)
        {
            Creator = creater;
            Target = target;
        }
        public override void Apply()
        {
            PreProcess();
            Target.hp.Minus(damage);
            Target.enemyAnimator.PlayTargetAnimation("damage",true);
            PostProcess();
        }
        /// <summary>
        /// 后置行为
        /// </summary>
        public override void PostProcess()
        {
            Debug.Log("-------------触发了后置行为(如吸血等)-----------");
            Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
            Target.ActionPointManager.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
        }
        /// <summary>
        /// 前置行为
        /// </summary>
        public override void PreProcess()
        {
            Creator.ActionPointManager.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
            Target.ActionPointManager.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            Debug.Log("------------触发了前置行为(如计算免伤等等)---------");
        }
    }
}