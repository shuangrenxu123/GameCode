using System.Collections.Generic;

namespace GOAP
{
    /// <summary>
    /// GOAP 规划器接口：从目标条件反向搜索，寻找由当前世界状态支持的
    /// 最低总 Cost Action 序列。抽象出接口以便替换不同的规划实现做性能对比。
    /// 性能统计等实现细节不属于契约，由具体实现自行暴露。
    /// </summary>
    public interface IGoapPlanner<T, V>
    {
        /// <summary>
        /// 为指定目标生成计划；无法找到计划时返回 null。
        /// </summary>
        /// <param name="availableActions">Agent 可用的全部 Action 集合。</param>
        /// <param name="worldState">当前世界状态。</param>
        /// <param name="goal">目标期望世界状态。</param>
        /// <returns>按执行顺序排列的 Action 队列；找不到计划时为 null。</returns>
        Queue<GoapAction<T, V>> Plan(
            HashSet<GoapAction<T, V>> availableActions,
            Dictionary<T, V> worldState,
            Dictionary<T, V> goal);
    }
}
