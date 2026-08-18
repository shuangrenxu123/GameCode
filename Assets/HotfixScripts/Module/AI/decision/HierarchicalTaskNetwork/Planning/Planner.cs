using System.Collections.Generic;
using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// HTN 规划器：递归前向分解 + 回溯（对应文档 12.4《寻找计划》）。
    /// 原始黑板 + 规划修改集；递归即回溯；失败分支只回滚规划修改。
    /// </summary>
    public class Planner
    {
        private readonly struct UndoEntry
        {
            public readonly int Key;
            public readonly bool Existed;
            public readonly BlackboardEntry OldValue;

            public UndoEntry(int key, bool existed, BlackboardEntry oldValue)
            {
                Key = key;
                Existed = existed;
                OldValue = oldValue;
            }
        }

        private readonly List<UndoEntry> _undoLog = new();
        private readonly Dictionary<int, BlackboardEntry> _workingChanges = new();
        private readonly Plan _plan = new();

        private Blackboard _sourceState;

        public Plan FindPlan(HTNDomain domain)
        {
            _sourceState = domain.worldState;
            _undoLog.Clear();
            _workingChanges.Clear();
            _plan.Clear();

            Plan result = null;
            try
            {
                if (DecomposeTask(domain.RootTask))
                {
                    result = _plan;
                }
                else
                {
                    _plan.Clear();
                }
            }
            finally
            {
                ClearPlanningState();
            }

            return result;
        }

        private bool DecomposeTask(Task task)
        {
            if (task is PrimitiveTask primitive)
            {
                if (!AreConditionsSatisfied(primitive.Preconditions))
                {
                    return false;
                }

                ApplyEffects(primitive);
                _plan.Add(primitive);
                return true;
            }

            CompoundTask compound = (CompoundTask)task;
            for (int i = 0; i < compound.Methods.Count; i++)
            {
                Method method = compound.Methods[i];
                if (!AreConditionsSatisfied(method.Conditions))
                {
                    continue;
                }

                int planMarker = _plan.Count;
                int undoMarker = _undoLog.Count;

                bool ok = true;
                foreach (Task subtask in method.Subtasks)
                {
                    if (!DecomposeTask(subtask))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    return true;
                }

                _plan.Truncate(planMarker);
                RollbackUndo(undoMarker);
            }

            return false;
        }

        private void ApplyEffects(PrimitiveTask primitive)
        {
            ApplyWithUndo(primitive.Effects);
            ApplyWithUndo(primitive.ExpectedEffects);
        }

        private void ApplyWithUndo(Blackboard effects)
        {
            foreach (KeyValuePair<int, BlackboardEntry> effect in effects.Entries)
            {
                bool existed = _workingChanges.TryGetValue(effect.Key, out BlackboardEntry oldValue);
                _undoLog.Add(new UndoEntry(effect.Key, existed, oldValue));
                _workingChanges[effect.Key] = effect.Value;
            }
        }

        private void RollbackUndo(int marker)
        {
            for (int i = _undoLog.Count - 1; i >= marker; i--)
            {
                UndoEntry e = _undoLog[i];
                if (e.Existed)
                {
                    _workingChanges[e.Key] = e.OldValue;
                }
                else
                {
                    _workingChanges.Remove(e.Key);
                }
            }

            _undoLog.RemoveRange(marker, _undoLog.Count - marker);
        }

        private bool AreConditionsSatisfied(Blackboard conditions)
        {
            foreach (KeyValuePair<int, BlackboardEntry> condition in conditions.Entries)
            {
                if (!TryGetPlanningEntry(condition.Key, out BlackboardEntry entry) ||
                    !entry.ValueEquals(condition.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryGetPlanningEntry(int key, out BlackboardEntry entry)
        {
            if (_workingChanges.TryGetValue(key, out entry))
            {
                return true;
            }

            return _sourceState.TryGetEntry(key, out entry);
        }

        private void ClearPlanningState()
        {
            _undoLog.Clear();
            _workingChanges.Clear();
            _sourceState = null;
        }
    }
}
