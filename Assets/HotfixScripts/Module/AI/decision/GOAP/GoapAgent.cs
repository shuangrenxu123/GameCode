using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class GoapAgent<T, V>
    {
        private Dictionary<T, V> worldState = new();
        public Dictionary<T, V> WorldState
        {
            get { return worldState; }
            set { worldState = value; }
        }

        private readonly HashSet<GoapAction<T, V>> availableActions;
        private ulong registeredActionMask;
        private Queue<GoapAction<T, V>> currentActions;
        public List<Goal<T, V>> goals;

        private readonly IGoapPlanner<T, V> planner;
        private float planDeltaTime = 1f;
        private float lastPlanTime;
        private bool running;

        public float LastPlanTime => lastPlanTime;

        public GoapAgent()
            : this(new GoapPlanner<T, V>())
        {
        }

        /// <summary>
        /// 使用自定义规划器实现构造 Agent，便于替换不同实现做性能对比。
        /// </summary>
        public GoapAgent(IGoapPlanner<T, V> planner)
        {
            availableActions = new HashSet<GoapAction<T, V>>();
            currentActions = new Queue<GoapAction<T, V>>();
            this.planner = planner ?? throw new System.ArgumentNullException(nameof(planner));
            goals = new List<Goal<T, V>>();
        }

        private bool HasActionPlan()
        {
            return currentActions.Count > 0;
        }

        public void AddAction(GoapAction<T, V> action)
        {
            if (action == null)
            {
                throw new System.ArgumentNullException(nameof(action));
            }

            if (availableActions.Contains(action))
            {
                return;
            }

            ValidateActionMask(action.ActionMask, nameof(action));
            if ((registeredActionMask & action.ActionMask) != 0UL)
            {
                throw new System.ArgumentException(
                    $"ActionMask {action.ActionMask} 已被其他 Action 使用。",
                    nameof(action));
            }

            availableActions.Add(action);
            registeredActionMask |= action.ActionMask;
        }

        public void RemoveAction(GoapAction<T, V> action)
        {
            if (action != null && availableActions.Remove(action))
            {
                registeredActionMask &= ~action.ActionMask;
            }
        }

        private static void ValidateActionMask(ulong actionMask, string parameterName)
        {
            if (actionMask == 0UL || (actionMask & (actionMask - 1UL)) != 0UL)
            {
                throw new System.ArgumentException(
                    $"ActionMask {actionMask} 必须非零且只能包含一个置位 bit。",
                    parameterName);
            }
        }

        public int GetAvailableActionsCount()
        {
            return availableActions.Count;
        }

        public bool BuildPlan(bool forcePlan = true)
        {
            if (goals == null || goals.Count == 0)
            {
                Debug.LogWarning("[GOAP] 没有可用目标，无法生成计划");
                return false;
            }

            if (running)
            {
                if (!forcePlan)
                {
                    return false;
                }

                AbortCurrentPlan();
            }

            if (Time.time - lastPlanTime < planDeltaTime && !forcePlan)
            {
                return false;
            }

            lastPlanTime = Time.time;

            foreach (Goal<T, V> goal in goals)
            {
                // 已经满足的高优先级 Goal 不应阻塞后续未满足 Goal。
                if (InState(goal.goal, worldState))
                {
                    continue;
                }

                Queue<GoapAction<T, V>> plan =
                    planner.Plan(availableActions, worldState, goal.goal);
                if (plan != null)
                {
                    currentActions = plan;
                    running = true;
                    return true;
                }
            }

            return false;
        }

        private void AbortCurrentPlan()
        {
            if (currentActions.Count > 0)
            {
                GoapAction<T, V> currentAction = currentActions.Peek();
                if (currentAction.Running)
                {
                    currentAction.PlanExit();
                }
            }

            currentActions.Clear();
            running = false;
        }

        private bool CanExecuteAction(GoapAction<T, V> action)
        {
            return InState(action.Preconditions, worldState) &&
                   action.CheckProceduralPreCondition(worldState);
        }

        public bool ForceBuildPlan()
        {
            lastPlanTime = 0f;
            return BuildPlan(true);
        }

        public void AddGoal(Goal<T, V> goal)
        {
            goals.Add(goal);
            SortGoalsByPriority();
        }

        public bool UpdateGoalPriority(Goal<T, V> goal, int priority)
        {
            if (goal == null || !goals.Contains(goal))
            {
                return false;
            }

            goal.SetPriority(priority);
            SortGoalsByPriority();
            return true;
        }

        private void SortGoalsByPriority()
        {
            goals.Sort((left, right) => right.Priority.CompareTo(left.Priority));
        }

        public void RunPlan()
        {
            if (!HasActionPlan())
            {
                running = false;
                return;
            }

            GoapAction<T, V> action = currentActions.Peek();
            if (!CanExecuteAction(action))
            {
                AbortCurrentPlan();
                BuildPlan(true);
                return;
            }

            if (!action.Running)
            {
                action.PlanEnter();
            }

            action.PlanExecute();

            if (action.IsDone)
            {
                action.PlanExit();
                currentActions.Dequeue();
                ApplyActionEffects(action);
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

        public void ApplyActionEffects(GoapAction<T, V> action)
        {
            foreach (KeyValuePair<T, V> effect in action.Effects)
            {
                worldState[effect.Key] = effect.Value;
            }
        }
    }
}
