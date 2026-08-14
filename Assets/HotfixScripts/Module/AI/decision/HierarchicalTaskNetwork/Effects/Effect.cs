using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 描述原子任务成功后对黑板产生的变化。
    /// </summary>
    public abstract class Effect
    {
        public abstract void ApplyToPlanning(Blackboard source, Blackboard changes);

        public abstract void ApplyToExecution(Blackboard blackboard);
    }
}
