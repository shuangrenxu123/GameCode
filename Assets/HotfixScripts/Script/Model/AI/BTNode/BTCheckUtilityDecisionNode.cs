using AIBlackboard;
using BT;
using UtilityAI;

namespace BT.Action
{
    /// <summary>
    /// 检查效用AI决策是否匹配指定名称
    /// </summary>
    public class BTCheckUtilityDecisionNode : BTAction
    {
        private readonly string targetOptionName;
        private UtilityDecision currentDecision;

        public BTCheckUtilityDecisionNode(string optionName)
        {
            targetOptionName = optionName;
        }

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
        }

        protected override BTResult Execute()
        {
            currentDecision = database.GetValue<EnemyAIDatabaseKey, UtilityDecision>(EnemyAIDatabaseKey.UtilityDecision);

            if (currentDecision.IsValid && currentDecision.OptionName == targetOptionName)
            {
                return BTResult.Success;
            }

            return BTResult.Failed;
        }
    }
}
