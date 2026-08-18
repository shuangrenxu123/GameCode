using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 由规划器生成的原子任务序列（按执行顺序）。
    /// 使用执行下标推进任务，保留完整任务序列供计划验证使用。
    /// </summary>
    public class Plan
    {
        private readonly List<PrimitiveTask> _tasks = new();
        private int _nextTaskIndex;

        /// <summary>完整的原子任务序列。</summary>
        public IReadOnlyList<PrimitiveTask> Tasks => _tasks;

        /// <summary>下一项待执行任务在完整计划中的下标。</summary>
        public int NextTaskIndex => _nextTaskIndex;

        /// <summary>尚未取出的任务数量。</summary>
        public int RemainingCount => _tasks.Count - _nextTaskIndex;

        internal int Count => _tasks.Count;

        /// <summary>向计划末尾追加一个原子任务。</summary>
        public void Add(PrimitiveTask task)
        {
            _tasks.Add(task);
        }

        /// <summary>取出下一个原子任务并推进执行下标；计划完成时返回 false。</summary>
        public bool TryGetNextTask(out PrimitiveTask task)
        {
            if (_nextTaskIndex >= _tasks.Count)
            {
                task = null;
                return false;
            }

            task = _tasks[_nextTaskIndex];
            _nextTaskIndex++;
            return true;
        }

        /// <summary>清空任务并重置执行位置，保留列表容量供后续规划复用。</summary>
        public void Clear()
        {
            _tasks.Clear();
            _nextTaskIndex = 0;
        }

        internal void Truncate(int count)
        {
            if (_tasks.Count > count)
            {
                _tasks.RemoveRange(count, _tasks.Count - count);
            }
        }
    }
}
