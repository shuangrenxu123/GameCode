namespace BT
{
    /// <summary>
    /// ѡ������ֱ����һ�������˳ɹ���
    /// </summary>
    public class BTPrioritySelector : BTNode
    {
        private BTNode activeChild;

        public BTPrioritySelector(BTPrecondition precondition = null) : base(precondition)
        {
        }

        /// <summary>
        /// �ҵ��Ƿ���һ���ӽڵ����ִ��
        /// </summary>
        /// <returns></returns>
        protected override bool DoEvaluate()
        {
            foreach (var i in children)
            {
                if (i.Evaluate())
                {
                    if (activeChild != null && activeChild != i)
                    {
                        activeChild.Clear();
                    }
                    activeChild = i;
                    return true;
                }
            }
            activeChild = null;
            return false;
        }

        public override void Clear()
        {
            if (activeChild != null)
            {
                activeChild.Clear();
                activeChild = null;
            }
        }

        public override BTResult Tick()
        {
            if (activeChild == null)
            {
                return BTResult.Ended;
            }
            BTResult result = activeChild.Tick();
            if (result == BTResult.Running)
                return result;
            else if (result == BTResult.Ended)
            {
                Clear();
                return BTResult.Ended;
            }
            else
            {
                int i = children.IndexOf(activeChild);
                if (i == children.Count - 1)
                {
                    Clear();
                    return BTResult.Fail;
                }
                activeChild = children[i + 1];
                return BTResult.Running;
            }
        }
    }
}
