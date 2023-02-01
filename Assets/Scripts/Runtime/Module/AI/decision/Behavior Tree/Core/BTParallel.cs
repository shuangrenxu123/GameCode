using System.Collections.Generic;

namespace BT
{
    /// <summary>
    ///并行节点。BTParallel 评估所有子级，如果其中任何一个未通过评估，则 BTParallel将失败。
    /// 添加子节点的顺寻很重要
    /// </summary>
    public class BTParallel : BTNode
    {
        protected List<BTResult> results;
        protected ParallelFunction func;
        public BTParallel(ParallelFunction func) : this(func, null)
        {

        }
        public BTParallel(ParallelFunction func, BTPrecondition precondition) : base(precondition)
        {
            this.func = func;
            results = new List<BTResult>();
        }

        protected override bool DoEvaluate()
        {
            foreach (var i in children)
            {
                if (!i.Evaluate())
                {
                    return false;
                }
            }
            return true;
        }

        public override BTResult Tick()
        {
            int count = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (results[i] == BTResult.Running)
                {
                    results[i] = children[i].Tick();
                }
                else
                {
                    if (func == ParallelFunction.And)
                    {
                        count++;
                    }
                    else
                    {
                        ResetResult();
                        return results[i];
                    }
                }
            }
            if (count == children.Count)
            {
                ResetResult();
                return BTResult.Ended;
            }
            return BTResult.Running;
        }

        /// <summary>
        /// 重置所有的子节点
        /// </summary>
        private void ResetResult()
        {
            for (int i = 0; i < results.Count; i++)
            {
                results[i] = BTResult.Running;
            }
        }
        public override void Clear()
        {
            ResetResult();
            foreach (var i in children)
            {
                i.Clear();
            }
        }

        public override void AddChild(BTNode node)
        {
            base.AddChild(node);
            results.Add(BTResult.Running);
        }
        public override void RemoveChild(BTNode node)
        {
            int id = children.IndexOf(node);
            results.RemoveAt(id);
            base.RemoveChild(node);
        }
    }
    public enum ParallelFunction
    {

        /// <summary>
        /// 等待所有的子节点都执行完
        /// </summary>
        And = 1,
        /// <summary>
        /// 只要有一个提前整完
        /// </summary>
        Or = 2,
    }
}
