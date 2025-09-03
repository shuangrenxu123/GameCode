using Fight;
using System.Collections.Generic;
using UnityEngine;

namespace HTN
{
    public class GameRoot : MonoBehaviour
    {
        protected DomainBase domain;
        protected Planner planner;
        protected PlanRunner planRunner;
        protected WorldState ws;
        protected WorldState previousWorldState;
        protected Queue<PrimitiveTask> currentPlan;
        protected float planningCooldown = 0.5f; // 规划冷却时间，减少频繁规划
        protected float lastPlanningTime = 0f;
        protected bool needsReplan = true;

        public void Start()
        {
            this.ws = new WorldState();
            //在其中添加具体的任务
            domain.Init();
            domain.BuildWorldState();
            //初始化规划器
            planner = new Planner();
            planner.Init(ws, domain);
            //初始化执行器
            planRunner = new PlanRunner(ws);
            currentPlan = null;
            previousWorldState = null;
        }
        private void Update()
        {
            // 检查世界状态是否改变，只有在改变时才重新规划
            if (previousWorldState == null || HasWorldStateChanged())
            {
                needsReplan = true;
                previousWorldState = new WorldState();
                // 复制当前世界状态
                previousWorldState.CopyFrom(ws);
            }

            // 如果需要重新规划且冷却时间已过
            if (needsReplan && Time.time - lastPlanningTime > planningCooldown)
            {
                planner.BuildPlan();
                currentPlan = planner.GetFinalTask();
                lastPlanningTime = Time.time;
                needsReplan = false;
            }

            // 执行当前计划
            if (currentPlan != null && currentPlan.Count > 0)
            {
                planRunner.RunPlan(currentPlan);
            }
        }

        // 检查世界状态是否发生重要变化
        private bool HasWorldStateChanged()
        {
            // 这里应该比较关键的世界状态属性
            // 简化版本：总是返回true表示需要重新规划（生产环境应优化）
            return true;
        }
    }
}