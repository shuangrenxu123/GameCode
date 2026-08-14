using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 由规划器生成的原子任务序列。
    /// </summary>
    public class Plan
    {
        public Queue<PrimitiveTask> Tasks { get; } = new();

        public void Add(PrimitiveTask task)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetNextTask(out PrimitiveTask task)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}
