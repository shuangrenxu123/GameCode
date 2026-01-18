using System;
using System.Collections.Generic;
namespace Fight
{
    public class ActionPointManager
    {
        Dictionary<ActionPointType, List<Action<CombatAction>>> actionCallback
            = new(4);
        /// <summary>
        /// 初始化行为。注册某个行为
        /// </summary>
        public void Init()
        {
            //ActionCallback.Add(ActionPointType.PreReceiveDamage, new List<Action<CombatAction>>(10));
            //ActionCallback.Add(ActionPointType.PostCauseDamage, new List<Action<CombatAction>>(10));
            for (int i = 0; i < (int)ActionPointType.Length; i++)
            {
                actionCallback.Add((ActionPointType)i, new List<Action<CombatAction>>(10));
            }
        }
        public void AddListener(ActionPointType type, Action<CombatAction> action)
        {
            actionCallback[type].Add(action);
        }
        public void RemoveListener(ActionPointType type, Action<CombatAction> action)
        {
            actionCallback[type].Remove(action);
        }
        public void TriggerActionPoint(ActionPointType type, CombatAction action)
        {
            if (actionCallback.ContainsKey(type))
            {
                foreach (var item in actionCallback[type])
                {
                    item.Invoke(action);
                }
            }
            else
            {
                UnityEngine.Debug.LogError(type + "  is null");
            }
        }
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
        ///<summary>
        ///恢复生命值之前
        /// </summary>
        PreRestoreHP,
        /// <summary>
        /// 恢复生命值之后
        /// </summary>
        PostRestoreHP,

        Length = 5,
    }
}
