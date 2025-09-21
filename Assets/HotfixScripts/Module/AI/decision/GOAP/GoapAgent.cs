using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class GoapAgent<T, V>
    {
        Dictionary<T, V> worldState = new(); //世界状态
        public Dictionary<T, V> WorldState { get { return worldState; } set { worldState = value; } }
        private HashSet<GoapAction<T, V>> availableActions;
        private Queue<GoapAction<T, V>> currentActions;

        public List<Goal<T, V>> goals;
        // private IGoap dataProvider;

        /// <summary>
        /// 默认构造函数，初始化内部集合
        /// </summary>
        public GoapAgent()
        {
            availableActions = new HashSet<GoapAction<T, V>>();
            currentActions = new Queue<GoapAction<T, V>>();
            planner = new GoapPlanner<T, V>();
            goals = new List<Goal<T, V>>();
        }

        /// <summary>
        /// 每次重新获得计划的间隔时间
        /// </summary>
        float planDeltaTime = 1f;


        //==========RunTime====================

        /// <summary>
        /// 是否正在计划中
        /// </summary>
        bool isPlanning = false;

        /// <summary>
        /// 上一次计划的时间
        /// </summary>
        public float LastPlanTime => lastPlanTime;
        float lastPlanTime = 0;

        private GoapPlanner<T, V> planner;

        private bool running = false;
        private void Start()
        {
            availableActions = new();
            currentActions = new();
            planner = new();

        }


        private bool HasActionPlan()
        {
            return currentActions.Count > 0;
        }

        public void AddAction(GoapAction<T, V> action)
        {
            availableActions.Add(action);
        }
        public void RemoveAction(GoapAction<T, V> action)
        {
            availableActions.Remove(action);
        }
        /// <summary>
        /// 获取可用Action的数量（用于测试验证）
        /// </summary>
        /// <returns>Action数量</returns>
        public int GetAvailableActionsCount()
        {
            return availableActions != null ? availableActions.Count : 0;
        }
        /// <summary>
        /// �Ƿ��ҵ��˺��ʵļƻ�
        /// </summary>
        /// <returns></returns>
        public bool BuildPlan(bool forcePlan = true)
        {
            if (running && !forcePlan)
            {
                return false;
            }
            if (Time.time - lastPlanTime < planDeltaTime && !forcePlan)
            {
                return false;
            }
            lastPlanTime = Time.time;

            Goal<T, V> goal = null;
            foreach (var g in goals)
            {
                if (goal == null)
                {
                    goal = g;
                }
                else
                {
                    if (goal.Priority < g.Priority)
                    {
                        goal = g;
                    }
                }
            }

            //��� Actions
            Queue<GoapAction<T, V>> plan = planner.Plan(availableActions, worldState, goal.goal);
            if (plan != null)
            {
                currentActions = plan;
                running = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ForceBuildPlan()
        {
            lastPlanTime = 0;
            return BuildPlan(true);
        }

        public void AddGoal(Goal<T, V> goal)
        {
            goals.Add(goal);
        }

        /// <summary>
        /// ִ��plan
        /// </summary>
        /// <returns>�Ƿ��������</returns>
        public void RunPlan()
        {
            if (!HasActionPlan())
            {
                running = false;
                return;
            }

            GoapAction<T, V> action = currentActions.Peek();

            if (action.IsDone())
            {
                action.PlanExit();
                // 在Action完成执行后应用其Effect
                currentActions.Dequeue();
                ApplyActionEffects(action);
            }
            if (action != null)
            {
                action.PlanExecute();

            }
        }
        private Dictionary<T, V> PopulateState
            (Dictionary<T, V> currentState,
            Dictionary<T, V> stateChange)
        {
            Dictionary<T, V> state = new Dictionary<T, V>(currentState);

            foreach (var change in stateChange)
            {
                if (state.ContainsKey(change.Key))
                {
                    // 假设 V 是 int，进行增量更新
                    if (typeof(V) == typeof(int))
                    {
                        int newVal = ((int)(object)state[change.Key]) + ((int)(object)change.Value);
                        state[change.Key] = (V)(object)newVal;
                    }
                    else
                    {
                        state[change.Key] = change.Value;
                    }
                }
                else
                {
                    state.Add(change.Key, change.Value);
                }
            }
            return state;
        }

        public void ApplyActionEffects(GoapAction<T, V> action)
        {
            worldState = PopulateState(worldState, action.Effects);
        }
    }
}

