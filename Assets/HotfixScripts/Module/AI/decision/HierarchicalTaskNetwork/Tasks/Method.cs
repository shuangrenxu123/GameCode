using System.Collections.Generic;
using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 完成复合任务的一种分解方法。
    /// 条件以数据（Blackboard 键值对）表示，子任务保持有序列表。
    /// 条件采用懒初始化：未使用时零分配。
    /// </summary>
    public class Method
    {
        public string Name { get; set; }

        private Blackboard _conditions;

        /// <summary>方法可选的期望状态。</summary>
        public Blackboard Conditions => _conditions ??= new();

        /// <summary>该方法的子任务（按执行顺序）。</summary>
        public List<Task> Subtasks { get; } = new();

        public bool AreConditionsSatisfied(Blackboard state)
        {
            return WorldStateOps.IsSatisfied(Conditions, state);
        }
    }
}
