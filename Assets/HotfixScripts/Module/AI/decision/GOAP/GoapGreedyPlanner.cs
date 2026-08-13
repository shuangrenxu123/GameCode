using System;
using System.Collections.Generic;
using AIBlackboard;

namespace GOAP
{
    /// <summary>
    /// 贪婪规划器：按成本升序深度优先搜索，找到第一个可行计划立即返回。
    ///
    /// 与 GoapPlanner 的取舍：
    /// - 不保证代价最小：返回第一个可行计划即止，不做全空间穷举证明最优；
    /// - 通常显著更快：省掉"找到计划后继续搜索更优解"的全部开销，
    ///   在存在大量重叠/排列的场景中优势尤其明显；
    /// - 启发式只用于终止判定：每个节点增量维护"尚未被 worldState 满足的
    ///   条件数"，归零即得到计划（不做候选重排序，保持单遍搜索的低开销）；
    /// - 受 MaxNodeExpansions 预算保护：预算耗尽直接返回 null，
    ///   因此"有解但没找到"是可能的（使用方已知晓该约定）。
    ///
    /// 用法：new GoapAgent(new GoapGreedyPlanner())。
    /// </summary>
    public class GoapGreedyPlanner : IGoapPlanner
    {
        private readonly List<GoapAction> usableActions = new();
        private readonly Dictionary<int, BlackboardEntry> workingConditions = new();
        private readonly List<ConditionChange> conditionChanges = new();
        private readonly List<ConditionChange> pendingEffectRemovals = new();
        private readonly List<GoapAction> currentPath = new();

        private Queue<GoapAction> foundPlan;
        private int remainingExpansions;

        /// <summary>
        /// 单次 Plan 允许展开的最大节点数，用于封顶最坏耗时。
        /// 预算耗尽时即使存在可行计划也返回 null。
        /// </summary>
        public int MaxNodeExpansions = 1024;

        public Queue<GoapAction> Plan(
            HashSet<GoapAction> availableActions,
            Blackboard worldState,
            Blackboard goal)
        {
            ResetSearch();

            try
            {
                CollectUsableActions(availableActions, worldState);
                InitializeWorkingConditions(goal);

                int rootUnsatisfied = CountUnsatisfied(worldState);
                if (rootUnsatisfied == 0)
                {
                    return new Queue<GoapAction>();
                }

                remainingExpansions = MaxNodeExpansions;
                Search(0UL, rootUnsatisfied, worldState);
                return foundPlan;
            }
            finally
            {
                ResetSearch();
            }
        }

        private void ResetSearch()
        {
            usableActions.Clear();
            workingConditions.Clear();
            conditionChanges.Clear();
            pendingEffectRemovals.Clear();
            currentPath.Clear();

            foundPlan = null;
            remainingExpansions = 0;
        }

        private void CollectUsableActions(
            HashSet<GoapAction> availableActions,
            Blackboard worldState)
        {
            ulong availableActionMask = 0UL;
            foreach (GoapAction action in availableActions)
            {
                if (action == null)
                {
                    throw new ArgumentException(
                        "availableActions 不能包含 null。",
                        nameof(availableActions));
                }

                ValidateActionMask(action.ActionMask, nameof(availableActions));
                if ((availableActionMask & action.ActionMask) != 0UL)
                {
                    throw new ArgumentException(
                        $"ActionMask {action.ActionMask} 被多个 Action 重复使用。",
                        nameof(availableActions));
                }

                availableActionMask |= action.ActionMask;

                if (action.cost >= 0f && action.CheckProceduralPreCondition(worldState))
                {
                    usableActions.Add(action);
                }
            }

            // 成本升序：贪婪搜索优先沿便宜分支下降，改善首个可行计划的质量。
            usableActions.Sort((left, right) => left.cost.CompareTo(right.cost));
        }

        private void InitializeWorkingConditions(Blackboard goal)
        {
            foreach (KeyValuePair<int, BlackboardEntry> condition in goal.Entries)
            {
                workingConditions.Add(condition.Key, condition.Value);
            }
        }

        /// <summary>
        /// 单遍深度优先：usableActions 已按成本升序，依次尝试回归，
        /// 第一个成功的分支立即深入；任一路径达成计划则逐层直接返回。
        /// </summary>
        private bool Search(
            ulong usedActionMask,
            int unsatisfied,
            Blackboard worldState)
        {
            if (remainingExpansions <= 0)
            {
                return false;
            }

            remainingExpansions--;

            foreach (GoapAction action in usableActions)
            {
                if ((usedActionMask & action.ActionMask) != 0UL)
                {
                    continue;
                }

                int rollbackMarker = conditionChanges.Count;
                if (!TryRegressInPlace(
                        action,
                        worldState,
                        unsatisfied,
                        out int childUnsatisfied))
                {
                    RollbackConditions(rollbackMarker);
                    continue;
                }

                currentPath.Add(action);
                bool planFound;

                if (childUnsatisfied == 0)
                {
                    foundPlan = new Queue<GoapAction>(currentPath.Count);
                    for (int index = currentPath.Count - 1; index >= 0; index--)
                    {
                        foundPlan.Enqueue(currentPath[index]);
                    }

                    planFound = true;
                }
                else
                {
                    planFound = Search(
                        usedActionMask | action.ActionMask,
                        childUnsatisfied,
                        worldState);
                }

                currentPath.RemoveAt(currentPath.Count - 1);
                RollbackConditions(rollbackMarker);

                if (planFound)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 原地回归一个 Action，并增量更新未满足条件数：
        /// 被 Effect 移除的条件若原本未被世界状态满足，计数 -1；
        /// 新加入的前置条件若不被世界状态满足，计数 +1。
        /// </summary>
        private bool TryRegressInPlace(
            GoapAction action,
            Blackboard worldState,
            int parentUnsatisfied,
            out int childUnsatisfied)
        {
            childUnsatisfied = parentUnsatisfied;

            // 先完整检查 Effect 并记录待移除项，避免发生冲突后才修改工作字典。
            // 注意：Precondition 阶段是边检查边修改的，若中途冲突直接返回 false，
            // 已做的修改依赖调用方按 rollbackMarker 回滚。
            pendingEffectRemovals.Clear();
            foreach (KeyValuePair<int, BlackboardEntry> effect in action.Effects.Entries)
            {
                if (!workingConditions.TryGetValue(effect.Key, out BlackboardEntry requiredEntry))
                {
                    continue;
                }

                if (!effect.Value.ValueEquals(requiredEntry))
                {
                    return false;
                }

                pendingEffectRemovals.Add(new ConditionChange(
                    effect.Key,
                    true,
                    requiredEntry));
            }

            if (pendingEffectRemovals.Count == 0)
            {
                return false;
            }

            foreach (ConditionChange removal in pendingEffectRemovals)
            {
                conditionChanges.Add(removal);
                workingConditions.Remove(removal.Key);

                if (!WorldSatisfies(worldState, removal.Key, removal.OldEntry))
                {
                    childUnsatisfied--;
                }
            }

            foreach (KeyValuePair<int, BlackboardEntry> precondition in action.Preconditions.Entries)
            {
                if (workingConditions.TryGetValue(precondition.Key, out BlackboardEntry requiredEntry))
                {
                    if (!precondition.Value.ValueEquals(requiredEntry))
                    {
                        return false;
                    }

                    continue;
                }

                conditionChanges.Add(new ConditionChange(
                    precondition.Key,
                    false,
                    null));
                workingConditions.Add(precondition.Key, precondition.Value);

                if (!WorldSatisfies(worldState, precondition.Key, precondition.Value))
                {
                    childUnsatisfied++;
                }
            }

            return true;
        }

        private void RollbackConditions(int marker)
        {
            for (int index = conditionChanges.Count - 1; index >= marker; index--)
            {
                ConditionChange change = conditionChanges[index];
                if (change.Existed)
                {
                    workingConditions[change.Key] = change.OldEntry;
                }
                else
                {
                    workingConditions.Remove(change.Key);
                }
            }

            if (conditionChanges.Count > marker)
            {
                conditionChanges.RemoveRange(
                    marker,
                    conditionChanges.Count - marker);
            }
        }

        private int CountUnsatisfied(Blackboard worldState)
        {
            int unsatisfied = 0;
            foreach (KeyValuePair<int, BlackboardEntry> condition in workingConditions)
            {
                if (!WorldSatisfies(worldState, condition.Key, condition.Value))
                {
                    unsatisfied++;
                }
            }

            return unsatisfied;
        }

        private static bool WorldSatisfies(
            Blackboard worldState,
            int keyId,
            BlackboardEntry entry)
        {
            return worldState.TryGetEntry(keyId, out BlackboardEntry stateEntry) &&
                   entry.ValueEquals(stateEntry);
        }

        private static void ValidateActionMask(ulong actionMask, string parameterName)
        {
            if (actionMask == 0UL || (actionMask & (actionMask - 1UL)) != 0UL)
            {
                throw new ArgumentException(
                    $"ActionMask {actionMask} 必须非零且只能包含一个置位 bit。",
                    parameterName);
            }
        }
    }
}
