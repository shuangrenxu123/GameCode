using System.Collections.Generic;

namespace GOAP
{
    public class Goal<T, V>
    {
        public Goal(Dictionary<T, V> goal, int p)
        {
            this.goal = goal;
            Priority = p;
        }

        /// <summary>
        /// 目标的期望世界状态。
        /// </summary>
        public Dictionary<T, V> goal;

        /// <summary>
        /// 目标的优先级。
        /// </summary>
        public int Priority { get; private set; }

        internal void SetPriority(int priority)
        {
            Priority = priority;
        }
    }
}
