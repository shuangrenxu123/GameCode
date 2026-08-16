using System.Collections.Generic;
using AIBlackboard;

namespace HTN
{
    /// <summary>
    /// HTN 规划器：递归前向分解 + 回溯（对应文档 12.4《寻找计划》）。
    /// 单黑板 + undo-log；递归即回溯；失败分支就地回滚。
    /// </summary>
    public class Planner
    {
        private readonly struct UndoEntry
        {
            public readonly int Key;
            public readonly BlackboardEntry OldValue;

            public UndoEntry(int key, BlackboardEntry oldValue)
            {
                Key = key;
                OldValue = oldValue;
            }
        }

        private readonly List<PrimitiveTask> _finalPlan = new();
        private readonly List<UndoEntry> _undoLog = new();

        private Blackboard _workingState;

        public Plan FindPlan(HTNDomain domain)
        {
            _workingState = domain.worldState.Clone();
            _finalPlan.Clear();
            _undoLog.Clear();

            if (!DecomposeTask(domain.RootTask))
            {
                return null;
            }

            Plan plan = new Plan();
            foreach (PrimitiveTask primitive in _finalPlan)
            {
                plan.Add(primitive);
            }

            return plan;
        }

        private bool DecomposeTask(Task task)
        {
            if (task is PrimitiveTask primitive)
            {
                if (!primitive.ArePreconditionsSatisfied(_workingState))
                {
                    return false;
                }

                ApplyEffects(primitive);
                _finalPlan.Add(primitive);
                return true;
            }

            CompoundTask compound = (CompoundTask)task;
            for (int i = 0; i < compound.Methods.Count; i++)
            {
                Method method = compound.Methods[i];
                if (!method.AreConditionsSatisfied(_workingState))
                {
                    continue;
                }

                int planMarker = _finalPlan.Count;
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

                _finalPlan.RemoveRange(planMarker, _finalPlan.Count - planMarker);
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
                // 存旧引用（不 clone）+ 直接替换条目（不原地改）：零分配，且旧引用不会被污染。
                _workingState.TryGetEntry(effect.Key, out BlackboardEntry old);
                _undoLog.Add(new UndoEntry(effect.Key, old));
                _workingState.ReplaceEntry(effect.Key, effect.Value);
            }
        }

        private void RollbackUndo(int marker)
        {
            for (int i = _undoLog.Count - 1; i >= marker; i--)
            {
                UndoEntry e = _undoLog[i];
                _workingState.ReplaceEntry(e.Key, e.OldValue);
            }

            _undoLog.RemoveRange(marker, _undoLog.Count - marker);
        }
    }
}
