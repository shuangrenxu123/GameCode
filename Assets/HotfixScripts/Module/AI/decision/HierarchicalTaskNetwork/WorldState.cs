using System.Collections.Generic;
using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// 世界状态操作工具：条件校验与效果应用（对齐 GOAP 的 InState / ApplyActionEffects）。
    /// </summary>
    internal static class WorldStateOps
    {
        /// <summary>
        /// 判断 conditions 中的每个键值对是否被 state 满足（键存在且值相等）。
        /// </summary>
        public static bool IsSatisfied(Blackboard conditions, Blackboard state)
        {
            foreach (KeyValuePair<int, BlackboardEntry> condition in conditions.Entries)
            {
                if (!state.TryGetEntry(condition.Key, out BlackboardEntry entry) ||
                    !entry.ValueEquals(condition.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 把 effects 中的每个键值对写入 target（等价于 GOAP 的 ApplyActionEffects）。
        /// </summary>
        public static void Apply(Blackboard effects, Blackboard target)
        {
            foreach (KeyValuePair<int, BlackboardEntry> effect in effects.Entries)
            {
                target.WriteEntry(effect.Key, effect.Value);
            }
        }
    }
}
