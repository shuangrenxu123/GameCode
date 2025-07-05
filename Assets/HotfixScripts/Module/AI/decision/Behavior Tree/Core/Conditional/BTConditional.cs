namespace BT
{
    /// <summary>
    /// 检查节点
    /// 被用作与特定的修饰节点中，需要自己在子类中实现一个检测
    /// 他最终会被用于 BTConditionEvaluator中来进行一个判断
    /// </summary>
    public abstract class BTConditional : BTDecorator
    {
        protected BTConditional(BTNode child) : base(child)
        {
        }

        public sealed override BTResult Tick()
        {
            if (Check())
            {
                return BTResult.Success;
            }
            else
            {
                return BTResult.Failed;
            }
        }
        public virtual bool Check()
        {
            return false;
        }
    }
}