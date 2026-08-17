using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 按顺序运行计划中的原子任务（对应文档 12.5《运行计划》）。
    /// 黑板通过构造函数注入，Tick / Abort 无参。
    ///
    /// Tick 返回"计划级"状态：Running=进行中，Success=全部完成，Failure=任务失败。
    /// </summary>
    public class PlanRunner
    {
        private readonly Blackboard _worldState;

        public Plan CurrentPlan { get; private set; }

        public PrimitiveTask CurrentTask { get; private set; }

        public PlanRunner(Blackboard worldState)
        {
            _worldState = worldState;
        }

        public void SetPlan(Plan plan)
        {
            CurrentPlan = plan;
            CurrentTask = null;
        }

        public TaskStatus Tick()
        {
            // 无当前任务 → 取下一个；计划空 → 全部完成。
            if (CurrentTask == null)
            {
                if (!CurrentPlan.TryGetNextTask(out PrimitiveTask next))
                {
                    return TaskStatus.Success;
                }

                if (!next.ArePreconditionsSatisfied(_worldState))
                {
                    return TaskStatus.Failure;
                }

                CurrentTask = next;
            }

            TaskStatus status = CurrentTask.Operator.Execute(_worldState);

            if (status == TaskStatus.Running)
            {
                return TaskStatus.Running;
            }

            if (status == TaskStatus.Success)
            {
                // 成功：把效果写进真实世界，然后取下一条任务。
                CurrentTask.ApplyEffects(_worldState);
                CurrentTask = null;
                return CurrentPlan.RemainingCount > 0 ? TaskStatus.Running : TaskStatus.Success;
            }

            // Failure：整个计划失败。
            return TaskStatus.Failure;
        }

        public void Abort()
        {
            if (CurrentTask != null)
            {
                CurrentTask.Operator.Abort(_worldState);
            }

            CurrentTask = null;
            CurrentPlan = null;
        }
    }
}
