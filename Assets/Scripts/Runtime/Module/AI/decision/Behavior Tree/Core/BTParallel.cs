using System.Collections.Generic;

namespace BT
{
    /// <summary>
    ///���нڵ㡣BTParallel ���������Ӽ�����������κ�һ��δͨ���������� BTParallel��ʧ�ܡ�
    /// ����ӽڵ��˳Ѱ����Ҫ
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
        /// �������е��ӽڵ�
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
        /// �ȴ����е��ӽڵ㶼ִ����
        /// </summary>
        And = 1,
        /// <summary>
        /// ֻҪ��һ����ǰ����
        /// </summary>
        Or = 2,
    }
}
