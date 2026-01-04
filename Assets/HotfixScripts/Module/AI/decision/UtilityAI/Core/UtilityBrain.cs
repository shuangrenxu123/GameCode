using System;
using System.Collections.Generic;
using AIBlackboard;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// 效用AI大脑 - 核心决策引擎
    /// 管理所有选项，定期评估并选择最优行为
    /// </summary>
    public class UtilityBrain
    {
        /// <summary>
        /// 所有可用选项
        /// </summary>
        private List<Option> options = new();

        /// <summary>
        /// 选项选择器
        /// </summary>
        private IOptionSelector selector;

        /// <summary>
        /// 黑板数据
        /// </summary>
        public Blackboard Blackboard { get; private set; }

        /// <summary>
        /// 当前选中的选项
        /// </summary>
        public Option CurrentOption { get; private set; }

        /// <summary>
        /// 决策间隔时间（秒）
        /// </summary>
        public float DecisionInterval { get; set; } = 0.5f;

        /// <summary>
        /// 是否允许中断当前动作
        /// </summary>
        public bool AllowInterrupt { get; set; } = true;

        /// <summary>
        /// 中断阈值（新选项分数超过当前选项多少比例才会中断）
        /// </summary>
        public float InterruptThreshold { get; set; } = 1.2f;

        private float lastDecisionTime;
        private bool isRunning;

        /// <summary>
        /// 当选项变化时触发
        /// </summary>
        public event Action<Option, Option> OnOptionChanged;

        public UtilityBrain(Blackboard blackboard = null, IOptionSelector selector = null)
        {
            Blackboard = blackboard ?? new Blackboard();
            this.selector = selector ?? new HighestScoreSelector();
        }

        /// <summary>
        /// 设置选项选择器
        /// </summary>
        public void SetSelector(IOptionSelector selector)
        {
            this.selector = selector ?? new HighestScoreSelector();
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        public void AddOption(Option option)
        {
            if (option != null && !options.Contains(option))
            {
                options.Add(option);
            }
        }

        /// <summary>
        /// 移除选项
        /// </summary>
        public bool RemoveOption(Option option)
        {
            return options.Remove(option);
        }

        /// <summary>
        /// 根据名称获取选项
        /// </summary>
        public Option GetOption(string name)
        {
            return options.Find(o => o.name == name);
        }

        /// <summary>
        /// 启动效用AI
        /// </summary>
        public void Start()
        {
            isRunning = true;
            lastDecisionTime = Time.time;
            // 启动时立即做一次决策
            MakeDecision();
        }

        /// <summary>
        /// 停止效用AI
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            CurrentOption = null;
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update()
        {
            if (!isRunning) return;

            // 检查是否需要做决策
            bool shouldDecide = false;

            if (CurrentOption == null)
            {
                shouldDecide = true;
            }
            else if (Time.time - lastDecisionTime >= DecisionInterval)
            {
                // 允许中断且检查是否有更好的选项
                if (AllowInterrupt)
                {
                    shouldDecide = CheckForBetterOption();
                }
                else
                {
                    // 如果不允许中断，我们仍然需要定期评估当前选项的分数，或者重新评估所有选项
                    // 这里简化为定期重新决策
                    shouldDecide = true;
                }
            }

            if (shouldDecide)
            {
                MakeDecision();
            }
        }

        /// <summary>
        /// 强制立即做出决策
        /// </summary>
        public void ForceDecision()
        {
            MakeDecision();
        }

        private void MakeDecision()
        {
            lastDecisionTime = Time.time;

            var newOption = selector.Select(options, Blackboard);

            // 如果没有有效选项，或者选项发生变化
            if (newOption != CurrentOption)
            {
                var oldOption = CurrentOption;
                CurrentOption = newOption;

                if (CurrentOption != null)
                {
                    CurrentOption.MarkExecuted();
                }

                // 更新黑板上的决策数据
                UpdateDecisionOnBlackboard();

                OnOptionChanged?.Invoke(oldOption, newOption);
            }
            else if (CurrentOption != null)
            {
                // 选项没变，但分数可能变了，更新黑板
                UpdateDecisionOnBlackboard();
            }
        }

        private void UpdateDecisionOnBlackboard()
        {
            var decision = UtilityDecision.Invalid;
            if (CurrentOption != null)
            {
                decision = new UtilityDecision
                {
                    OptionName = CurrentOption.name,
                    Score = CurrentOption.LastScore,
                    Timestamp = Time.time
                };
            }
        }

        private bool CheckForBetterOption()
        {
            if (CurrentOption == null) return true;

            // 重新评估当前选项的分数
            float currentScore = CurrentOption.Evaluate(Blackboard);

            foreach (var option in options)
            {
                if (option == CurrentOption) continue;

                float score = option.Evaluate(Blackboard);
                if (score > currentScore * InterruptThreshold)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
