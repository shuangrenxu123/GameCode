using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 验证任务或方法是否适用于当前黑板状态。
    /// </summary>
    public abstract class Condition
    {
        public abstract bool IsSatisfied(Blackboard source, Blackboard changes);
    }
}
