using AIBlackboard;
using BT;
using UnityEngine;
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
        string lastDecision = "";

        public override void Activate(Blackboard database)
        {
            base.Activate(database);
            utilityBrain = database.GetValue<EnemyAIDatabaseKey, UtilityBrain>(utilityBrainKey);
        }

        protected override BTResult Execute()
        {
            if (utilityBrain == null)
            {
                Debug.LogError("[效用AI更新] 效用AI大脑为空！");
                return BTResult.Failed;
            }

            utilityBrain.Update();

            // 获取并打印当前决策
            var decision = database.GetValue<EnemyAIDatabaseKey, UtilityDecision>(EnemyAIDatabaseKey.UtilityDecision);

            // 每次都输出决策信息（临时调试）
            Debug.Log($"[效用AI更新] 当前决策: {decision.OptionName} (有效: {decision.IsValid})");

            // 检查黑板中的关键数据
            bool playerVisible = database.GetValue(EnemyAIDatabaseKey.PlayerVisible, false);
            float playerDistance = database.GetValue(EnemyAIDatabaseKey.PlayerDistance, float.MaxValue);
            Debug.Log($"[效用AI更新] 黑板数据 - 玩家可见: {playerVisible}, 距离: {playerDistance:F2}");

            // 只在决策变化时输出日志
            if (lastDecision != decision.OptionName)
            {
                Debug.Log($"[效用AI更新] 决策变化: {lastDecision} -> {decision.OptionName}");
                lastDecision = decision.OptionName;
            }

            return BTResult.Success; // 修改为Success，让行为树继续执行后续分支
        }
    }
}
