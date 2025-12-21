using BT;
using BT.Action;

namespace Enemy.AI
{
    /// <summary>
    /// 轻巡者行为树：
    /// 1. 优先检查是否需要翻滚（受击响应）
    /// 2. 根据Utility决策执行巡逻/追击/轻攻击
    /// </summary>
    public class PatrolSkirmisherBT : BTTree
    {
        public override void SetNode()
        {
            var rootNode = new BTSequence();

            // 1. Update Utility Brain
            var utilityNode = new BTUtilityUpdateNode();
            rootNode.AddChild(utilityNode);

            // 2. 优先级选择器
            var selector = new BTSelector();
            rootNode.AddChild(selector);

            // 2.0 翻滚分支（最高优先级）
            var rollSequence = new BTSequence();
            rollSequence.AddChild(new BTCheckShouldRollNode());
            rollSequence.AddChild(new BTRollNode());
            selector.AddChild(rollSequence);

            // 2.1 巡逻分支 - 使用减速巡逻节点
            var patrolSequence = new BTSequence();
            patrolSequence.AddChild(new BTCheckUtilityDecisionNode("Patrol"));
            patrolSequence.AddChild(new BTSlowPatrolNode());
            selector.AddChild(patrolSequence);

            // 2.2 追击分支
            var chaseSequence = new BTSequence();
            chaseSequence.AddChild(new BTCheckUtilityDecisionNode("Chase"));
            chaseSequence.AddChild(new BTChaseNode());
            selector.AddChild(chaseSequence);

            // 2.3 轻攻击分支
            var lightAttackSequence = new BTSequence();
            lightAttackSequence.AddChild(new BTCheckUtilityDecisionNode("LightAttack"));
            lightAttackSequence.AddChild(new BTLightAttackNode());
            selector.AddChild(lightAttackSequence);

            root = rootNode;
        }
    }
}
