using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 仅用于规划和计划验证的预期效果。
    /// </summary>
    public abstract class ExpectedEffect
    {
        public abstract void ApplyToPlanning(Blackboard source, Blackboard changes);
    }
}
