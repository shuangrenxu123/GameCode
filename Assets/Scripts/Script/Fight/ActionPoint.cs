using System;
using System.Collections.Generic;
namespace Fight
{


    public class ActionPoint
    {
        public List<Action<CombatAction>> Listeners = new List<Action<CombatAction>>();
    }
    public enum ActionPointType
    {
        /// <summary>
        /// 造成伤害前
        /// </summary>
        PreCauseDamage,
        /// <summary>
        /// 承受伤害前
        /// </summary>
        PreReceiveDamage,
        /// <summary>
        /// 造成伤害后
        /// </summary>
        PostCauseDamage,
        /// <summary>
        /// 承受伤害后
        /// </summary>
        PostReceiveDamage,
    }
}
