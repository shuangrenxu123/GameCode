using AIBlackboard;

namespace HTN
{
    public class HTNDomain
    {
        /// <summary>规划的根复合任务（领域数据）。</summary>
        public CompoundTask RootTask { get; set; }
        public readonly Blackboard worldState;
        private readonly Planner _planner;
        private readonly PlanRunner _runner;

        /// <summary>
        /// 由外层 AI 注入世界状态；决策层只消费、不拥有黑板。
        /// </summary>
        public HTNDomain(Blackboard state)
        {
            worldState = state ?? throw new System.ArgumentNullException(nameof(state));
            _planner = new();
            _runner = new PlanRunner(worldState);
        }

        /// <summary>当前计划（无计划时为 null）。</summary>
        public Plan CurrentPlan => _runner.CurrentPlan;

        /// <summary>当前正在执行的原子任务（无时为 null）。</summary>
        public PrimitiveTask CurrentTask => _runner.CurrentTask;

        /// <summary>是否存在正在执行的计划。</summary>
        public bool HasPlan => _runner.CurrentPlan != null;

        /// <summary>
        /// 找计划并装载：返回生成的计划；找不到可行计划返回 null。
        /// 若当前已有计划，请先 <see cref="InvalidatePlan"/>。
        /// </summary>
        public Plan FindPlan()
        {
            Plan plan = _planner.FindPlan(this);
            if (plan != null)
            {
                _runner.SetPlan(plan);
            }

            return plan;
        }

        /// <summary>
        /// 推进当前计划一步。无计划时返回 Failure，不负责找计划。
        /// </summary>
        public TaskStatus Tick()
        {
            if (!HasPlan)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = _runner.Tick();

            // 计划结束（成功或失败）：清理现场，交由调用方决定何时重规划。
            if (status != TaskStatus.Running)
            {
                _runner.Abort();
            }

            return status;
        }

        /// <summary>
        /// 主动放弃当前计划（例如传感器检测到世界状态变化），之后重新规划。
        /// </summary>
        public void InvalidatePlan()
        {
            if (HasPlan)
            {
                _runner.Abort();
            }
        }

        /// <summary>
        /// 完全重置：放弃当前计划并清空根任务。
        /// </summary>
        public void Reset()
        {
            InvalidatePlan();
            RootTask = null;
        }
    }
}
