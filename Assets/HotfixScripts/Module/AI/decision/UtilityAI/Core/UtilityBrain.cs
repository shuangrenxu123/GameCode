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
        /// 当前执行的选项
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

        /// <summary>
        /// 当动作完成时触发
        /// </summary>
        public event Action<Option, ActionState> OnActionCompleted;

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
        }

        /// <summary>
        /// 停止效用AI
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            if (CurrentOption != null)
            {
                CurrentOption.action?.Abort(Blackboard);
                CurrentOption = null;
            }
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
                // 当前动作已完成
                if (CurrentOption.action.State != ActionState.Running)
                {
                    shouldDecide = true;
                }
                // 允许中断且检查是否有更好的选项
                else if (AllowInterrupt)
                {
                    shouldDecide = CheckForBetterOption();
                }
            }

            if (shouldDecide)
            {
                MakeDecision();
            }

            // 执行当前动作
            ExecuteCurrentAction();
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

            if (newOption == null) return;

            // 如果选择了不同的选项
            if (newOption != CurrentOption)
            {
                var oldOption = CurrentOption;

                // 中断旧动作
                if (CurrentOption != null && CurrentOption.action.State == ActionState.Running)
                {
                    CurrentOption.action.Abort(Blackboard);
                }

                CurrentOption = newOption;
                CurrentOption.MarkExecuted();
                CurrentOption.action.Enter(Blackboard);

                OnOptionChanged?.Invoke(oldOption, newOption);
            }
        }

        private bool CheckForBetterOption()
        {
            if (CurrentOption == null) return true;

            float currentScore = CurrentOption.LastScore;

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

        private void ExecuteCurrentAction()
        {
            if (CurrentOption?.action == null) return;

            var state = CurrentOption.action.Execute(Blackboard);

            if (state != ActionState.Running)
            {
                CurrentOption.action.Exit(Blackboard);
                OnActionCompleted?.Invoke(CurrentOption, state);

                // 动作完成后清空当前选项，下次Update会选择新选项
                CurrentOption = null;
            }
        }

        /// <summary>
        /// 获取所有选项的调试信息
        /// </summary>
        public List<(string name, float score, bool isCurrent)> GetDebugInfo()
        {
            var info = new List<(string, float, bool)>();

            foreach (var option in options)
            {
                option.Evaluate(Blackboard);
                info.Add((option.name, option.LastScore, option == CurrentOption));
            }

            info.Sort((a, b) => b.score.CompareTo(a.score));
            return info;
        }
    }
}
