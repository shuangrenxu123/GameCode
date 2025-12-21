using AIBlackboard;
using BT;
using UnityEngine;
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

            Debug.Log($"[效用决策检查] 目标: {targetOptionName}, 当前: {currentDecision.OptionName}, 有效: {currentDecision.IsValid}");

            if (currentDecision.IsValid && currentDecision.OptionName == targetOptionName)
            {
                Debug.Log($"[效用决策检查] 成功 - 执行 {targetOptionName}");
                return BTResult.Success;
            }

            return BTResult.Failed;
        }
    }
}
