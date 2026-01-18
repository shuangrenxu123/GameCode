using System.Collections.Generic;
using ObjectPool;
using UnityEngine;
namespace Fight
{
    public static class CombatActionFactor
    {
        public static T CreateActionAndExecute<T>
            (CombatEntity creator, List<CombatEntity> target, int baseValue)
             where T : CombatAction, new()
        {
            var action = ReferenceManager.Instance.Spawn<T>();
            action.Setup(creator, target);
            action.Apply(baseValue);
            return action;
        }
    }
}
