using System.Collections.Generic;
using BT;
using UnityEngine;
namespace Fight
{
    public class AttackNode : BTAction
    {
        protected override BTResult Execute()
        {
            var target = database.GetData<List<CombatEntity>>("Target");
            // CombatActionFactor.CreateActionAndExecute<DamageAction>();
            return BTResult.Success;
        }
    }
}
