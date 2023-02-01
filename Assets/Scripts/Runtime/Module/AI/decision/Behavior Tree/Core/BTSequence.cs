namespace BT
{
    /// <summary>
    /// 顺序节点，从左往右执行子节点，知道所有的都成功或者有一个返回失败
    /// </summary>
    public class BTSequence : BTNode
    {
        /// <summary>
        /// 当前激活的节点
        /// </summary>
        private BTNode activeChild;

        /// <summary>
        /// 激活节点的序号
        /// </summary>
        private int activeIndex = -1;

        public BTSequence(BTPrecondition precondition) : base(precondition) { }

        /// <summary>
        /// 判断节点是否可以执行
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
        /// 执行一次，会首先执行第一个
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
