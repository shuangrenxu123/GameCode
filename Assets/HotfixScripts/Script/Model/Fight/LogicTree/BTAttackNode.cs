using System.Collections.Generic;
using AIBlackboard;
using BT;
using Enemy;
using UnityEngine;
namespace BT.Action
{
    public class BTAttackNode : BTAction
    {

        private IEnemyBrain entityBrain;
        public override void Activate(Blackboard database)
        {
            base.Activate(database);

            entityBrain = database.GetValue<IEnemyBrain>("entityBrain");
        }
        protected override BTResult Execute()
        {
            entityBrain.characterActions.attack.value = true;

            return BTResult.Success;
        }
    }
}
