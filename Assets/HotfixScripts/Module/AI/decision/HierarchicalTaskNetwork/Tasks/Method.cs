using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 完成复合任务的一种分解方法。
    /// </summary>
    public class Method
    {
        public string Name { get; set; }

        public List<Condition> Conditions { get; } = new();

        public List<Task> Subtasks { get; } = new();

        public bool AreConditionsSatisfied(
            AIBlackboard.Blackboard source,
            AIBlackboard.Blackboard changes)
        {
            throw new System.NotImplementedException();
        }
    }
}
