using System.Collections.Generic;
using BT;
using Enemy;
using UnityEngine;
namespace BT.Action
{
    public class BTAttackNode : BTAction
    {

        private IEnemyBrain entityBrain;
        public override void Activate(DataBase<string, object> database)
        {
            base.Activate(database);

            entityBrain = database.GetData<IEnemyBrain>("entityBrain");
        }
        protected override BTResult Execute()
        {
            entityBrain.characterActions.attack.value = true;

            return BTResult.Success;
        }
    }
}
