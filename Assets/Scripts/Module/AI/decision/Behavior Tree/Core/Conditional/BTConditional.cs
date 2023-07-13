namespace BT
{
    /// <summary>
    /// 该类为一种特殊节点，检查节点
    /// 被用作与特定的修饰节点中，需要自己在子类中实现一个检测
    /// 他最终会被用于 BTConditionEvaluator中来进行一个判断
    /// </summary>
    public abstract class BTConditional : BTNode
    {
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