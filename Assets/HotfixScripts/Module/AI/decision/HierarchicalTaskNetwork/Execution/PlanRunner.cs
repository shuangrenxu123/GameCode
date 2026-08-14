using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 按顺序运行计划中的原子任务。
    /// </summary>
    public class PlanRunner
    {
        public Plan CurrentPlan { get; private set; }

        public PrimitiveTask CurrentTask { get; private set; }

        public void SetPlan(Plan plan)
        {
            throw new System.NotImplementedException();
        }

        public TaskStatus Tick(Blackboard blackboard)
        {
            throw new System.NotImplementedException();
        }

        public void Abort(Blackboard blackboard)
        {
            throw new System.NotImplementedException();
        }
    }
}
