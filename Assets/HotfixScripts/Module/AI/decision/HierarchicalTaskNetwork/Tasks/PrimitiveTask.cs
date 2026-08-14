using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 可以直接执行并进入最终计划的原子任务。
    /// </summary>
    public class PrimitiveTask : Task
    {
        public Operator Operator { get; set; }

        public List<Condition> Preconditions { get; } = new();

        public List<Effect> Effects { get; } = new();

        public List<ExpectedEffect> ExpectedEffects { get; } = new();

        public bool ArePreconditionsSatisfied(
            AIBlackboard.Blackboard source,
            AIBlackboard.Blackboard changes)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyEffectsToPlanning(
            AIBlackboard.Blackboard source,
            AIBlackboard.Blackboard changes)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyExpectedEffectsToPlanning(
            AIBlackboard.Blackboard source,
            AIBlackboard.Blackboard changes)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyEffectsToExecution(AIBlackboard.Blackboard blackboard)
        {
            throw new System.NotImplementedException();
        }
    }
}
