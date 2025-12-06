using AIBlackboard;
using BT;
using UtilityAI;

namespace BT.Action
{
    /// <summary>
    /// 调用效用AI进行行为决策的BT节点
    /// </summary>
    public class BTUtilityUpdateNode : BTAction
    {
        public EnemyAIDatabaseKey utilityBrainKey = EnemyAIDatabaseKey.UtilityBrain;
        UtilityBrain utilityBrain;

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            utilityBrain = database.GetValue<EnemyAIDatabaseKey, UtilityBrain>(utilityBrainKey);
        }

        protected override BTResult Execute()
        {
            if (utilityBrain == null)
            {
                return BTResult.Failed;
            }

            utilityBrain.Update();
            return BTResult.Running;
        }
    }
}
