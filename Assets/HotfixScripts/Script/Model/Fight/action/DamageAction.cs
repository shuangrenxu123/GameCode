using System.Collections.Generic;
using Fight.Number;
using UnityEngine;
namespace Fight
{
    public class DamageAction : CombatAction
    {
        public int damage;

        public override void Setup(CombatEntity creator, List<CombatEntity> target)
        {
            base.Setup(creator, target);
        }
        public override void Apply(int baseValue)
        {
            damage = baseValue;
            foreach (var target in Target)
            {
                PreProcess(Creator, target);
                Process(Creator, target);
                PostProcess(Creator, target);
            }
            Release();
        }

        public void Process(CombatEntity c, CombatEntity t)
        {
            t.hp.Minus(damage);
        }
        /// <summary>
        /// 后置行为
        /// </summary>
        protected override void PostProcess(CombatEntity c, CombatEntity t)
        {
            Creator.TriggerActionPoint(ActionPointType.PostCauseDamage, this);
            PostCreatorAction?.Invoke(this);

            foreach (var action in PostTargetActions)
            {
                action?.Invoke(this);
            }

            foreach (var target in Target)
            {
                target.TriggerActionPoint(ActionPointType.PostReceiveDamage, this);
            }
        }
        /// <summary>
        /// 前置行为
        /// </summary>
        protected override void PreProcess(CombatEntity c, CombatEntity t)
        {
            Creator.TriggerActionPoint(ActionPointType.PreCauseDamage, this);
            PreCreatorAction?.Invoke(this);
            foreach (var action in PreTargetActions)
            {
                action?.Invoke(this);
            }

            foreach (var target in Target)
            {
                target.TriggerActionPoint(ActionPointType.PreReceiveDamage, this);
            }
        }
    }
}