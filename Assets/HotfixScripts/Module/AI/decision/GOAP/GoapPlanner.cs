using System;
using System.Collections.Generic;
using AIBlackboard;

namespace GOAP
{
    readonly struct SearchStateSignature : IEquatable<SearchStateSignature>
    {
        public readonly int ConditionsHash;
        public readonly int ConditionCount;
        public readonly ulong UsedActionMask;

        public SearchStateSignature(
            int conditionsHash,
            int conditionCount,
            ulong usedActionMask)
        {
            ConditionsHash = conditionsHash;
            ConditionCount = conditionCount;
            UsedActionMask = usedActionMask;
        }

        public bool Equals(SearchStateSignature other)
        {
            return ConditionsHash == other.ConditionsHash &&
                   ConditionCount == other.ConditionCount &&
                   UsedActionMask == other.UsedActionMask;
        }

        public override bool Equals(object obj)
        {
            return obj is SearchStateSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ConditionsHash;
                hash = (hash * 397) ^ ConditionCount;
                hash = (hash * 397) ^ UsedActionMask.GetHashCode();
                return hash;
            }
        }
    }

    readonly struct ConditionChange
    {
        public readonly int Key;
        public readonly bool Existed;
        public readonly BlackboardEntry OldEntry;

        public ConditionChange(int key, bool existed, BlackboardEntry oldEntry)
        {
            Key = key;
            Existed = existed;
            OldEntry = oldEntry;
        }
    }

    readonly struct ConditionSnapshot
    {
        public readonly int Key;
        public readonly BlackboardEntry Entry;

        public ConditionSnapshot(int key, BlackboardEntry entry)
        {
            Key = key;
            Entry = entry;
        }
    }

    struct CachedSearchState
    {
        public int ConditionsOffset;
        public int ConditionCount;
        public float BestCost;
        public int NextCollisionIndex;

        public CachedSearchState(
            int conditionsOffset,
            int conditionCount,
            float bestCost,
            int nextCollisionIndex)
        {
            ConditionsOffset = conditionsOffset;
            ConditionCount = conditionCount;
            BestCost = bestCost;
            NextCollisionIndex = nextCollisionIndex;
        }
    }

    public class GoapPlanner : IGoapPlanner
    {
        private readonly List<GoapAction> usableActions = new();
        private readonly Dictionary<int, BlackboardEntry> workingConditions = new();
        private readonly List<ConditionChange> conditionChanges = new();
        private readonly List<ConditionChange> pendingEffectRemovals = new();
        private readonly List<ConditionSnapshot> conditionSnapshots = new();
        private readonly List<CachedSearchState> cachedSearchStates = new();
        private readonly Dictionary<SearchStateSignature, int> stateBucketHeads = new();
        private readonly List<GoapAction> currentPath = new();
        private readonly List<GoapAction> bestPath = new();

        private float bestCost;
        private int workingConditionsHash;

        /// <summary>
        /// 从目标条件开始反向回归，寻找能够由当前世界状态支持的最低成本 Action 序列。
        /// 搜索过程只复用一个条件字典，并通过修改栈在递归返回时恢复状态。
        /// 对工作条件中的 Entry 只整体替换、从不修改其 Value，因此快照与回滚
        /// 直接保存 Entry 引用即可；也绝不触碰 worldState 本体，避免误触
        /// Blackboard 的 OnValueChanged 事件。
        /// </summary>
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
                RegisterOrImproveCurrentState(0f, 0UL);

                if (InState(workingConditions, worldState))
                {
                    bestCost = 0f;
                }
                else
                {
                    BuildGraph(0f, 0UL, worldState);
                }

                if (bestCost == float.MaxValue)
                {
                    return null;
                }

                Queue<GoapAction> plan = new(bestPath.Count);
                for (int index = bestPath.Count - 1; index >= 0; index--)
                {
                    plan.Enqueue(bestPath[index]);
                }

                return plan;
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
            conditionSnapshots.Clear();
            cachedSearchStates.Clear();
            stateBucketHeads.Clear();
            currentPath.Clear();
            bestPath.Clear();

            bestCost = float.MaxValue;
            workingConditionsHash = 0;
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

                // 负成本会破坏最低成本剪枝，因此不允许参与规划。
                if (action.cost >= 0f && action.CheckProceduralPreCondition(worldState))
                {
                    usableActions.Add(action);
                }
            }

            // 低成本 Action 先行，让 bestCost 尽早下降以剪掉更多分支。
            usableActions.Sort((left, right) => left.cost.CompareTo(right.cost));
        }

        private void InitializeWorkingConditions(Blackboard goal)
        {
            foreach (KeyValuePair<int, BlackboardEntry> condition in goal.Entries)
            {
                workingConditions.Add(condition.Key, condition.Value);
                workingConditionsHash ^=
                    GetConditionHash(condition.Key, condition.Value);
            }
        }

        private void BuildGraph(
            float runningCost,
            ulong usedActionMask,
            Blackboard worldState)
        {
            foreach (GoapAction action in usableActions)
            {
                if ((usedActionMask & action.ActionMask) != 0UL)
                {
                    continue;
                }

                float nextCost = runningCost + action.cost;
                if (nextCost >= bestCost)
                {
                    continue;
                }

                int rollbackMarker = conditionChanges.Count;
                int previousHash = workingConditionsHash;

                if (!TryRegressInPlace(action))
                {
                    RollbackConditions(rollbackMarker, previousHash);
                    continue;
                }

                ulong nextUsedActionMask = usedActionMask | action.ActionMask;
                if (!RegisterOrImproveCurrentState(nextCost, nextUsedActionMask))
                {
                    RollbackConditions(rollbackMarker, previousHash);
                    continue;
                }

                currentPath.Add(action);

                if (InState(workingConditions, worldState))
                {
                    bestCost = nextCost;
                    bestPath.Clear();
                    bestPath.AddRange(currentPath);
                }
                else
                {
                    BuildGraph(nextCost, nextUsedActionMask, worldState);
                }

                currentPath.RemoveAt(currentPath.Count - 1);
                RollbackConditions(rollbackMarker, previousHash);
            }
        }

        private bool TryRegressInPlace(GoapAction action)
        {
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
                workingConditionsHash ^=
                    GetConditionHash(removal.Key, removal.OldEntry);
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
                workingConditionsHash ^=
                    GetConditionHash(precondition.Key, precondition.Value);
            }

            return true;
        }

        private void RollbackConditions(int marker, int previousHash)
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

            workingConditionsHash = previousHash;
        }

        private bool RegisterOrImproveCurrentState(
            float runningCost,
            ulong usedActionMask)
        {
            SearchStateSignature signature = new(
                workingConditionsHash,
                workingConditions.Count,
                usedActionMask);

            int collisionIndex = -1;
            if (stateBucketHeads.TryGetValue(signature, out int stateIndex))
            {
                collisionIndex = stateIndex;
                while (stateIndex >= 0)
                {
                    CachedSearchState cachedState = cachedSearchStates[stateIndex];
                    if (CurrentConditionsEqual(cachedState))
                    {
                        if (cachedState.BestCost <= runningCost)
                        {
                            return false;
                        }

                        cachedState.BestCost = runningCost;
                        cachedSearchStates[stateIndex] = cachedState;
                        return true;
                    }

                    stateIndex = cachedState.NextCollisionIndex;
                }
            }

            int conditionsOffset = conditionSnapshots.Count;
            foreach (KeyValuePair<int, BlackboardEntry> condition in workingConditions)
            {
                conditionSnapshots.Add(new ConditionSnapshot(
                    condition.Key,
                    condition.Value));
            }

            int newStateIndex = cachedSearchStates.Count;

            cachedSearchStates.Add(new CachedSearchState(
                conditionsOffset,
                workingConditions.Count,
                runningCost,
                collisionIndex));
            stateBucketHeads[signature] = newStateIndex;
            return true;
        }

        private bool CurrentConditionsEqual(CachedSearchState cachedState)
        {
            if (cachedState.ConditionCount != workingConditions.Count)
            {
                return false;
            }

            int end = cachedState.ConditionsOffset + cachedState.ConditionCount;
            for (int index = cachedState.ConditionsOffset; index < end; index++)
            {
                ConditionSnapshot condition = conditionSnapshots[index];
                if (!workingConditions.TryGetValue(condition.Key, out BlackboardEntry entry) ||
                    !condition.Entry.ValueEquals(entry))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetConditionHash(int keyId, BlackboardEntry entry)
        {
            unchecked
            {
                return (keyId * 397) ^ entry.GetValueHash();
            }
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

        private bool InState(
            Dictionary<int, BlackboardEntry> conditions,
            Blackboard state)
        {
            // 条件 key 互不重复，状态必须至少包含同样多的条目才可能全部满足。
            if (conditions.Count > state.Count)
            {
                return false;
            }

            foreach (KeyValuePair<int, BlackboardEntry> condition in conditions)
            {
                if (!state.TryGetEntry(condition.Key, out BlackboardEntry entry) ||
                    !entry.ValueEquals(condition.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
