using System.Collections.Generic;
using AIBlackboard;
using UnityEngine;

namespace GOAP
{
    public class GoapAgent
    {
        private Blackboard worldState = new();
        public Blackboard WorldState
        {
            get { return worldState; }
            set { worldState = value; }
        }

        private readonly HashSet<GoapAction> availableActions;
        private ulong registeredActionMask;
        private Queue<GoapAction> currentActions;
        public List<Goal> goals;

        private readonly IGoapPlanner planner;
        private float planDeltaTime = 1f;
        private float lastPlanTime;
        private bool running;

        public float LastPlanTime => lastPlanTime;

        public GoapAgent()
            : this(new GoapPlanner())
        {
        }

        /// <summary>
        /// 使用自定义规划器实现构造 Agent，便于替换不同实现做性能对比。
        /// </summary>
        public GoapAgent(IGoapPlanner planner)
        {
            availableActions = new HashSet<GoapAction>();
            currentActions = new Queue<GoapAction>();
            this.planner = planner ?? throw new System.ArgumentNullException(nameof(planner));
            goals = new List<Goal>();
        }

        private bool HasActionPlan()
        {
            return currentActions.Count > 0;
        }

        public void AddAction(GoapAction action)
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

        public void RemoveAction(GoapAction action)
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

            foreach (Goal goal in goals)
            {
                // 已经满足的高优先级 Goal 不应阻塞后续未满足 Goal。
                if (InState(goal.goal, worldState))
                {
                    continue;
                }

                Queue<GoapAction> plan =
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
                GoapAction currentAction = currentActions.Peek();
                if (currentAction.Running)
                {
                    currentAction.PlanExit();
                }
            }

            currentActions.Clear();
            running = false;
        }

        private bool CanExecuteAction(GoapAction action)
        {
            return InState(action.Preconditions, worldState) &&
                   action.CheckProceduralPreCondition(worldState);
        }

        public bool ForceBuildPlan()
        {
            lastPlanTime = 0f;
            return BuildPlan(true);
        }

        public void AddGoal(Goal goal)
        {
            goals.Add(goal);
            SortGoalsByPriority();
        }

        public bool UpdateGoalPriority(Goal goal, int priority)
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

            GoapAction action = currentActions.Peek();
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

        private bool InState(Blackboard conditions, Blackboard state)
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

        public void ApplyActionEffects(GoapAction action)
        {
            foreach (KeyValuePair<int, BlackboardEntry> effect in action.Effects.Entries)
            {
                worldState.WriteEntry(effect.Key, effect.Value);
            }
        }
    }
}
