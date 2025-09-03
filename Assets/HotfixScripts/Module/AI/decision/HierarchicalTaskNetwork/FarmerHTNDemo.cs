using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTN
{
    public class FarmerHTNDemo : MonoBehaviour
    {
        private FarmerDomain domain;
        private Planner planner;
        private PlanRunner planRunner;
        private WorldState ws;

        private Queue<PrimitiveTask> currentPlan;
        private float planningCooldown = 2.0f; // 规划冷却时间
        private float lastPlanningTime = 0f;
        private float lastLogTime = 0f;
        private float logInterval = 3.0f; // 状态日志间隔
        private float taskExecutionInterval = 1.5f; // 任务执行间隔
        private float lastTaskExecutionTime = 0f;
        private bool isExecuting = false; // 是否正在执行任务

        void Start()
        {
            Debug.Log("=== 农夫HTN系统演示开始 ===\n");

            // 初始化世界状态
            ws = new WorldState();

            // 初始化世界状态
            ws = new WorldState();

            // 创建农夫领域并初始化
            domain = new FarmerDomain(ws);
            domain.Init();

            // 初始化规划器和执行器
            planner = new Planner();
            planner.Init(ws, domain);

            planRunner = new PlanRunner(ws);
            currentPlan = null;

            // 首次规划
            DoPlanning();
        }

        void Update()
        {
            // 定期重新规划
            if (Time.time - lastPlanningTime > planningCooldown && !isExecuting)
            {
                DoPlanning();
                lastPlanningTime = Time.time;
            }

            // 逐步执行当前计划中的任务
            if (currentPlan != null && currentPlan.Count > 0 && !isExecuting)
            {
                ExecuteNextTask();
            }

            // 定期输出状态
            if (Time.time - lastLogTime > logInterval)
            {
                LogCurrentState();
                lastLogTime = Time.time;
            }

            // 任务执行冷却结束
            if (isExecuting && Time.time - lastTaskExecutionTime > taskExecutionInterval)
            {
                isExecuting = false;
            }
        }

        private void ExecuteNextTask()
        {
            if (isExecuting) return;

            isExecuting = true;
            lastTaskExecutionTime = Time.time;

            // 移除执行时的多余日志，保持简洁
            var result = planRunner.RunSingleTask(currentPlan);

            if (result == HTNResults.fail)
            {
                Debug.LogError("任务执行失败！需要重新规划...");
                isExecuting = false; // 重置执行状态
                DoPlanning();
            }
            else if (currentPlan.Count == 0)
            {
                Debug.Log("计划执行完成！等待下次规划...");
                // 这里可以选择立即规划或等待定时器
                isExecuting = false;
                // 可以选择自动继续规划或等待
                // DoPlanning();
            }
        }

        private void DoPlanning()
        {
            // 静默规划，不输出组织过程日志
            planner.BuildPlan();
            currentPlan = planner.GetFinalTask();

            if (currentPlan == null || currentPlan.Count == 0)
            {
                Debug.LogError("【关键问题】规划失败！没有获得任何任务。检查领域初始化和方法条件。");
                currentPlan = null;
            }
        }

        private void LogCurrentState()
        {
            int stamina = ws.GetInt(WSProperties.WS_FarmerStamina);
            int wood = ws.GetInt(WSProperties.WS_WoodCount);
            int axeDurability = ws.GetInt(WSProperties.WS_AxeDurability);

            // 智能状态分析
            string axeStatus = axeDurability > 8 ? "🟩 优秀" :
                              axeDurability > 5 ? "🟨 中等" :
                              axeDurability > 2 ? "🟧 需要关注" :
                              axeDurability > 0 ? "🔴 即将损坏" : "🚫 彻底损坏";

            string staminaStatus = stamina > 70 ? "🟩 精力充沛" :
                                  stamina > 40 ? "🟨 状态良好" :
                                  stamina > 20 ? "🟧 需要休息" : "🔴 体力不支";

            string efficiency = (stamina > 50 && axeDurability > 0) ? "⚡ 高效工作模式" :
                               (stamina > 20 && axeDurability == 0) ? "🐌 徒手工作模式" :
                               (stamina <= 20 || axeDurability <= 2) ? "🔧 需要维修/休息" : "⚠️ 资源不足";

            Debug.Log($"📊 当前状态概览:");
            Debug.Log($"   体力: {stamina}/100 {staminaStatus}");
            Debug.Log($"   木材: {wood} 个");
            Debug.Log($"   斧头: {axeDurability}/10 {axeStatus}");
            Debug.Log($"   效率: {efficiency}");
            Debug.Log($"   任务队列: {currentPlan?.Count ?? 0} 个等待执行");

            // 智能建议输出
            if (axeDurability <= 3 && wood > 0 && stamina > 30)
            {
                Debug.Log("💡 建议: 斧头耐久不足，应该修斧头!");
            }
            else if (stamina <= 25 && axeDurability > 0)
            {
                Debug.Log("💡 建议: 体力不足，应该休息恢复!");
            }
            else if (stamina > 50 && axeDurability > 5)
            {
                Debug.Log("💡 建议: 状态良好，可以高效工作!");
            }

            Debug.Log("--8<------------------------------");
        }
    }
}