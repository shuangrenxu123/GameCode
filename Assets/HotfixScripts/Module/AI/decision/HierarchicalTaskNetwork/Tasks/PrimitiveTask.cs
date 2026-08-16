using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 可以直接执行并进入最终计划的原子任务。
    /// 前置条件 / 效果 / 预期效果均以数据（Blackboard 键值对）表示，与 GOAP 一致。
    /// 条件/效果采用懒初始化：未使用时零分配。
    /// </summary>
    public class PrimitiveTask : Task
    {
        /// <summary>执行阶段运行的 Operator。</summary>
        public Operator Operator { get; set; }

        private Blackboard _preconditions;
        private Blackboard _effects;
        private Blackboard _expectedEffects;

        /// <summary>前置条件：任务运行前必须满足的期望状态。</summary>
        public Blackboard Preconditions => _preconditions ??= new();

        /// <summary>效果：任务成功后写入的状态变化。</summary>
        public Blackboard Effects => _effects ??= new();

        /// <summary>预期效果：仅规划期写入的预期变化（文档 12.7），执行期不写。</summary>
        public Blackboard ExpectedEffects => _expectedEffects ??= new();

        public bool ArePreconditionsSatisfied(Blackboard state)
        {
            return WorldStateOps.IsSatisfied(Preconditions, state);
        }

        /// <summary>
        /// 把 Effects 写入指定状态（规划期写工作状态、执行期写真实状态，二者操作相同、目标不同）。
        /// </summary>
        public void ApplyEffects(Blackboard state)
        {
            WorldStateOps.Apply(Effects, state);
        }
    }
}
