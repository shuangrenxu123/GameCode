using AIBlackboard;

namespace UtilityAI
{
    /// <summary>
    /// 考量接口 - 用于评估某个因素对决策的影响
    /// 每个Consideration返回0-1之间的分数
    /// </summary>
    public interface IConsideration
    {
        /// <summary>
        /// 考量名称，用于调试
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 计算该考量的效用分数
        /// </summary>
        /// <param name="blackboard">AI黑板数据</param>
        /// <returns>0-1之间的分数，0表示完全不考虑，1表示最高优先级</returns>
        float Evaluate(Blackboard blackboard);
    }
}
