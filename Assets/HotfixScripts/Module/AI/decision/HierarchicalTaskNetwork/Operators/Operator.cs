using AIBlackboard;

namespace HTN
{
    public enum TaskStatus
    {
        Running,
        Success,
        Failure,
    }

    /// <summary>
    /// 原子任务在计划执行阶段运行的操作。
    /// </summary>
    public abstract class Operator
    {
        public abstract TaskStatus Execute(Blackboard blackboard);

        public abstract void Abort(Blackboard blackboard);
    }
}
