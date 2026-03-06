using AIBlackboard;

namespace AI.UtilityAI
{
    /// <summary>
    /// 仅由一个 UtilityConsideration 组成的节点。
    /// </summary>
    public sealed class SingleConsiderationUtilityNode<T> : UtilityNode<T> where T : struct, System.Enum
    {
        private UtilityConsideration consideration;

        public SingleConsiderationUtilityNode(T type, Blackboard blackboard, UtilityConsideration consideration = null)
            : base(type, blackboard)
        {
            this.consideration = consideration;
        }

        public SingleConsiderationUtilityNode<T> SetConsideration(UtilityConsideration consideration)
        {
            this.consideration = consideration;
            return this;
        }

        public override bool CheckCondition()
        {
            return base.CheckCondition() && (consideration?.CheckCondition() ?? false);
        }

        public override float CalculateScore()
        {
            return consideration?.CalculateScore() ?? 0f;
        }
    }
}
