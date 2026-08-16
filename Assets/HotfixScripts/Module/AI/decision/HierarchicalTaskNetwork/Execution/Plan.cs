using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 由规划器生成的原子任务序列（按执行顺序）。
    /// 本身是无逻辑的数据容器，只负责存放与取出原子任务。
    /// </summary>
    public class Plan
    {
        /// <summary>底层队列（按加入顺序执行）。</summary>
        public Queue<PrimitiveTask> Tasks { get; } = new();

        /// <summary>向计划末尾追加一个原子任务。</summary>
        public void Add(PrimitiveTask task)
        {
            Tasks.Enqueue(task);
        }

        /// <summary>取出并移除下一个原子任务；计划为空时返回 false。</summary>
        public bool TryGetNextTask(out PrimitiveTask task)
        {
            if (Tasks.Count == 0)
            {
                task = null;
                return false;
            }

            task = Tasks.Dequeue();
            return true;
        }

        /// <summary>清空计划。</summary>
        public void Clear()
        {
            Tasks.Clear();
        }
    }
}
