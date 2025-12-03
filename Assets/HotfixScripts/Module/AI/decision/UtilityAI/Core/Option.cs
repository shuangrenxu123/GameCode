using System;
using System.Collections.Generic;
using AIBlackboard;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// 考量组合方式
    /// </summary>
    public enum ConsiderationCombineMode
    {
        /// <summary>
        /// 相乘（默认，任一考量为0则总分为0）
        /// </summary>
        Multiply,
        /// <summary>
        /// 取平均值
        /// </summary>
        Average,
        /// <summary>
        /// 取最小值
        /// </summary>
        Min,
        /// <summary>
        /// 取最大值
        /// </summary>
        Max,
        /// <summary>
        /// 加权平均
        /// </summary>
        WeightedAverage
    }

    /// <summary>
    /// 效用AI选项 - 代表AI可以选择的一个行为选项
    /// 包含多个考量因素和一个动作
    /// </summary>
    [Serializable]
    public class Option
    {
        /// <summary>
        /// 选项名称
        /// </summary>
        public string name;

        /// <summary>
        /// 基础权重（用于调整该选项的整体优先级）
        /// </summary>
        public float baseWeight = 1f;

        /// <summary>
        /// 考量组合方式
        /// </summary>
        public ConsiderationCombineMode combineMode = ConsiderationCombineMode.Multiply;

        /// <summary>
        /// 该选项的所有考量因素
        /// </summary>
        private List<ConsiderationEntry> considerations = new();

        /// <summary>
        /// 执行的动作
        /// </summary>
        public IAction action;

        /// <summary>
        /// 最后一次计算的分数（用于调试）
        /// </summary>
        public float LastScore { get; private set; }

        /// <summary>
        /// 是否启用该选项
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// 冷却时间（秒）
        /// </summary>
        public float cooldown = 0f;

        private float lastExecuteTime = float.MinValue;

        public Option(string name, IAction action)
        {
            this.name = name;
            this.action = action;
        }

        /// <summary>
        /// 添加考量因素
        /// </summary>
        public Option AddConsideration(IConsideration consideration, float weight = 1f)
        {
            considerations.Add(new ConsiderationEntry
            {
                consideration = consideration,
                weight = weight
            });
            return this;
        }

        /// <summary>
        /// 计算该选项的效用分数
        /// </summary>
        public float Evaluate(Blackboard blackboard)
        {
            if (!enabled)
            {
                LastScore = 0f;
                return 0f;
            }

            // 检查冷却
            if (cooldown > 0 && Time.time - lastExecuteTime < cooldown)
            {
                LastScore = 0f;
                return 0f;
            }

            if (considerations.Count == 0)
            {
                LastScore = baseWeight;
                return baseWeight;
            }

            float score = CalculateCombinedScore(blackboard);

            // 应用补偿因子（解决多考量相乘导致分数过低的问题）
            if (combineMode == ConsiderationCombineMode.Multiply && considerations.Count > 1)
            {
                float compensation = 1f - (1f / considerations.Count);
                score = score + (1f - score) * compensation * score;
            }

            LastScore = score * baseWeight;
            return LastScore;
        }

        private float CalculateCombinedScore(Blackboard blackboard)
        {
            float totalWeight = 0f;
            float weightedSum = 0f;

            switch (combineMode)
            {
                case ConsiderationCombineMode.Multiply:
                    float product = 1f;
                    foreach (var entry in considerations)
                    {
                        float value = entry.consideration.Evaluate(blackboard);
                        product *= value;
                        if (product <= 0f) return 0f; // 短路优化
                    }
                    return product;

                case ConsiderationCombineMode.Average:
                    float sum = 0f;
                    foreach (var entry in considerations)
                    {
                        sum += entry.consideration.Evaluate(blackboard);
                    }
                    return sum / considerations.Count;

                case ConsiderationCombineMode.WeightedAverage:
                    foreach (var entry in considerations)
                    {
                        float value = entry.consideration.Evaluate(blackboard);
                        weightedSum += value * entry.weight;
                        totalWeight += entry.weight;
                    }
                    return totalWeight > 0f ? weightedSum / totalWeight : 0f;

                case ConsiderationCombineMode.Min:
                    float min = float.MaxValue;
                    foreach (var entry in considerations)
                    {
                        float value = entry.consideration.Evaluate(blackboard);
                        if (value < min) min = value;
                    }
                    return min == float.MaxValue ? 0f : min;

                case ConsiderationCombineMode.Max:
                    float max = float.MinValue;
                    foreach (var entry in considerations)
                    {
                        float value = entry.consideration.Evaluate(blackboard);
                        if (value > max) max = value;
                    }
                    return max == float.MinValue ? 0f : max;

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 标记该选项被执行
        /// </summary>
        public void MarkExecuted()
        {
            lastExecuteTime = Time.time;
        }

        /// <summary>
        /// 重置冷却
        /// </summary>
        public void ResetCooldown()
        {
            lastExecuteTime = float.MinValue;
        }

        [Serializable]
        private struct ConsiderationEntry
        {
            public IConsideration consideration;
            public float weight;
        }
    }
}
