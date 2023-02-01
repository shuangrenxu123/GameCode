namespace BT
{
    /// <summary>
    /// ˳��ڵ㣬��������ִ���ӽڵ㣬֪�����еĶ��ɹ�������һ������ʧ��
    /// </summary>
    public class BTSequence : BTNode
    {
        /// <summary>
        /// ��ǰ����Ľڵ�
        /// </summary>
        private BTNode activeChild;

        /// <summary>
        /// ����ڵ�����
        /// </summary>
        private int activeIndex = -1;

        public BTSequence(BTPrecondition precondition) : base(precondition) { }

        /// <summary>
        /// �жϽڵ��Ƿ����ִ��
        /// </summary>
        /// <returns></returns>
        protected override bool DoEvaluate()
        {
            if (activeChild != null)
            {
                bool result = activeChild.Evaluate();
                if (!result)
                {
                    activeChild.Clear();
                    activeChild = null;
                    activeIndex = -1;
                }
                return result;
            }
            else
            {
                return children[0].Evaluate();
            }
        }
        /// <summary>
        /// ִ��һ�Σ�������ִ�е�һ��
        /// </summary>
        /// <returns></returns>
        public override BTResult Tick()
        {
            if (activeChild == null)
            {
                activeChild = children[0];
                activeIndex = 0;
            }
            BTResult result = activeChild.Tick();
            if (result == BTResult.Ended)
            {
                activeIndex++;
                if (activeIndex >= children.Count)
                {
                    activeChild.Clear();
                    activeChild = null;
                    activeIndex = -1;
                }
                else
                {
                    activeChild.Clear();
                    activeChild = children[activeIndex];
                    result = BTResult.Running;
                }
            }
            else if (result == BTResult.Fail)
            {
                result = BTResult.Fail;
                Clear();
            }
            return result;
        }
        public override void Clear()
        {
            if (activeChild != null)
            {
                //activeChild.Clear();
                activeChild = null;
                activeIndex = -1;
            }
            foreach (var i in children)
            {
                i.Clear();
            }
        }
    }
}
