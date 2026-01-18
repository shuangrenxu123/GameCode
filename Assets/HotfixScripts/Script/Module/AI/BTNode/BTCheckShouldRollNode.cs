using AIBlackboard;
using BT;

namespace BT.Action
{
    /// <summary>
    /// 检查是否应该翻滚的节点
    /// </summary>
    public class BTCheckShouldRollNode : BTAction
    {
        public override void Activate(Blackboard database)
        {
            base.Activate(database);
        }

        protected override BTResult Execute()
        {
            bool shouldRoll = database.GetValue(EnemyAIDatabaseKey.ShouldRoll, false);

            if (shouldRoll)
            {
                return BTResult.Success;
            }

            return BTResult.Failed;
        }
    }
}
