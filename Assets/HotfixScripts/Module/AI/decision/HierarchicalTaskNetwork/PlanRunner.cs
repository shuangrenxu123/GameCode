using System.Collections.Generic;
using UnityEngine;

namespace HTN
{
    public class PlanRunner
    {
        WorldState state;
        public PlanRunner(WorldState state)
        {
            this.state = state;
        }

        /// <summary>
        /// 执行单个任务（每次只执行一个任务，逐步运行）
        /// </summary>
        public HTNResults RunSingleTask(Queue<PrimitiveTask> tasks)
        {
            if (tasks == null || tasks.Count == 0 || state == null)
            {
                Debug.LogWarning("PlanRunner.RunSingleTask: Invalid parameters for plan execution");
                return HTNResults.fail;
            }

            PrimitiveTask task = tasks.Dequeue();

            if (task == null)
            {
                Debug.LogError("PlanRunner.RunSingleTask: Encountered null task in plan queue");
                return HTNResults.fail;
            }

            var res = task.Execute();
            task.ApplyEffects(state);

            // 移除任务执行的具体时间日志，保持简洁
            return res;
        }

        /// <summary>
        /// 执行整个计划（一次性执行所有任务）
        /// </summary>
        public void RunPlan(Queue<PrimitiveTask> tasks)
        {
            if (tasks == null || tasks.Count == 0 || state == null)
            {
                Debug.LogWarning("PlanRunner.RunPlan: Invalid parameters for plan execution");
                return;
            }

            PrimitiveTask task = null;
            while (tasks.Count > 0)
            {
                task = tasks.Dequeue();

                if (task == null)
                {
                    Debug.LogError("PlanRunner.RunPlan: Encountered null task in plan queue");
                    continue;
                }

                var res = task.Execute();
                task.ApplyEffects(state);

                Debug.Log($"[任务执行] {task.Name} -> {res}");

                if (res == HTNResults.fail)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 获取剩余任务数量
        /// </summary>
        public int GetRemainingTaskCount(Queue<PrimitiveTask> tasks)
        {
            return tasks?.Count ?? 0;
        }
    }
}
