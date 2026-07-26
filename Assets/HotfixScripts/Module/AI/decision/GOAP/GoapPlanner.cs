using System;
using System.Collections.Generic;

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

    readonly struct ConditionChange<T, V>
    {
        public readonly T Key;
        public readonly bool Existed;
        public readonly V OldValue;

        public ConditionChange(T key, bool existed, V oldValue)
        {
            Key = key;
            Existed = existed;
            OldValue = oldValue;
        }
    }

    readonly struct ConditionSnapshot<T, V>
    {
        public readonly T Key;
        public readonly V Value;

        public ConditionSnapshot(T key, V value)
        {
            Key = key;
            Value = value;
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

    public class GoapPlanner<T, V>
    {
        private readonly HashSet<GoapAction<T, V>> usableActions = new();
        private readonly Dictionary<T, V> workingConditions = new();
        private readonly List<ConditionChange<T, V>> conditionChanges = new();
        private readonly List<ConditionSnapshot<T, V>> conditionSnapshots = new();
        private readonly List<CachedSearchState> cachedSearchStates = new();
        private readonly Dictionary<SearchStateSignature, int> stateBucketHeads = new();
        private readonly List<GoapAction<T, V>> currentPath = new();
        private readonly List<GoapAction<T, V>> bestPath = new();

        private float bestCost;
        private int workingConditionsHash;

        public int LastExpandedNodeCount { get; private set; }
        public int LastDeduplicatedNodeCount { get; private set; }
        public int LastHashCollisionStateCount { get; private set; }

        /// <summary>
        /// 从目标条件开始反向回归，寻找能够由当前世界状态支持的最低成本 Action 序列。
        /// 搜索过程只复用一个条件字典，并通过修改栈在递归返回时恢复状态。
        /// </summary>
        public Queue<GoapAction<T, V>> Plan(
            HashSet<GoapAction<T, V>> availableActions,
            Dictionary<T, V> worldState,
            Dictionary<T, V> goal)
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

                Queue<GoapAction<T, V>> plan = new(bestPath.Count);
                for (int index = bestPath.Count - 1; index >= 0; index--)
                {
                    plan.Enqueue(bestPath[index]);
                }

                return plan;
            }
            finally
            {
                ClearTransientSearchData();
            }
        }

        private void ResetSearch()
        {
            usableActions.Clear();
            workingConditions.Clear();
            conditionChanges.Clear();
            conditionSnapshots.Clear();
            cachedSearchStates.Clear();
            stateBucketHeads.Clear();
            currentPath.Clear();
            bestPath.Clear();

            bestCost = float.MaxValue;
            workingConditionsHash = 0;
            LastExpandedNodeCount = 0;
            LastDeduplicatedNodeCount = 0;
            LastHashCollisionStateCount = 0;
        }

        private void ClearTransientSearchData()
        {
            usableActions.Clear();
            workingConditions.Clear();
            conditionChanges.Clear();
            conditionSnapshots.Clear();
            cachedSearchStates.Clear();
            stateBucketHeads.Clear();
            currentPath.Clear();
            bestPath.Clear();
            workingConditionsHash = 0;
        }

        private void CollectUsableActions(
            HashSet<GoapAction<T, V>> availableActions,
            Dictionary<T, V> worldState)
        {
            ulong availableActionMask = 0UL;
            foreach (GoapAction<T, V> action in availableActions)
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
        }

        private void InitializeWorkingConditions(Dictionary<T, V> goal)
        {
            foreach (KeyValuePair<T, V> condition in goal)
            {
                workingConditions.Add(condition.Key, condition.Value);
                workingConditionsHash ^=
                    GetConditionHash(condition.Key, condition.Value);
            }
        }

        private bool BuildGraph(
            float runningCost,
            ulong usedActionMask,
            Dictionary<T, V> worldState)
        {
            bool foundPlan = false;
            LastExpandedNodeCount++;

            foreach (GoapAction<T, V> action in usableActions)
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
                    LastDeduplicatedNodeCount++;
                    RollbackConditions(rollbackMarker, previousHash);
                    continue;
                }

                currentPath.Add(action);

                if (InState(workingConditions, worldState))
                {
                    bestCost = nextCost;
                    bestPath.Clear();
                    bestPath.AddRange(currentPath);
                    foundPlan = true;
                }
                else if (BuildGraph(nextCost, nextUsedActionMask, worldState))
                {
                    foundPlan = true;
                }

                currentPath.RemoveAt(currentPath.Count - 1);
                RollbackConditions(rollbackMarker, previousHash);
            }

            return foundPlan;
        }

        private bool TryRegressInPlace(GoapAction<T, V> action)
        {
            bool satisfiesCondition = false;

            // 先完整检查 Effect，避免发生冲突后才修改工作字典。
            foreach (KeyValuePair<T, V> effect in action.Effects)
            {
                if (!workingConditions.TryGetValue(effect.Key, out V requiredValue))
                {
                    continue;
                }

                if (!EqualityComparer<V>.Default.Equals(effect.Value, requiredValue))
                {
                    return false;
                }

                satisfiesCondition = true;
            }

            if (!satisfiesCondition)
            {
                return false;
            }

            foreach (KeyValuePair<T, V> effect in action.Effects)
            {
                if (workingConditions.TryGetValue(effect.Key, out V requiredValue) &&
                    EqualityComparer<V>.Default.Equals(effect.Value, requiredValue))
                {
                    conditionChanges.Add(new ConditionChange<T, V>(
                        effect.Key,
                        true,
                        requiredValue));
                    workingConditions.Remove(effect.Key);
                    workingConditionsHash ^=
                        GetConditionHash(effect.Key, requiredValue);
                }
            }

            foreach (KeyValuePair<T, V> precondition in action.Preconditions)
            {
                if (workingConditions.TryGetValue(precondition.Key, out V requiredValue))
                {
                    if (!EqualityComparer<V>.Default.Equals(
                            precondition.Value,
                            requiredValue))
                    {
                        return false;
                    }

                    continue;
                }

                conditionChanges.Add(new ConditionChange<T, V>(
                    precondition.Key,
                    false,
                    default));
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
                ConditionChange<T, V> change = conditionChanges[index];
                if (change.Existed)
                {
                    workingConditions[change.Key] = change.OldValue;
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
            foreach (KeyValuePair<T, V> condition in workingConditions)
            {
                conditionSnapshots.Add(new ConditionSnapshot<T, V>(
                    condition.Key,
                    condition.Value));
            }

            int newStateIndex = cachedSearchStates.Count;
            if (collisionIndex >= 0)
            {
                LastHashCollisionStateCount++;
            }

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
                ConditionSnapshot<T, V> condition = conditionSnapshots[index];
                if (!workingConditions.TryGetValue(condition.Key, out V value) ||
                    !EqualityComparer<V>.Default.Equals(condition.Value, value))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetConditionHash(T key, V value)
        {
            int keyHash = EqualityComparer<T>.Default.GetHashCode(key);
            int valueHash = EqualityComparer<V>.Default.GetHashCode(value);

            unchecked
            {
                return (keyHash * 397) ^ valueHash;
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

        private bool InState(Dictionary<T, V> conditions, Dictionary<T, V> state)
        {
            foreach (KeyValuePair<T, V> condition in conditions)
            {
                if (!state.TryGetValue(condition.Key, out V value) ||
                    !EqualityComparer<V>.Default.Equals(value, condition.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
