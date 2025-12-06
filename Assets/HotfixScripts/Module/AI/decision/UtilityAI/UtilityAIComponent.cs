using System.Collections.Generic;
using AIBlackboard;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// 效用AI组件 - 挂载到GameObject上的MonoBehaviour包装器
    /// </summary>
    public class UtilityAIComponent : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private string currentOptionName = "None";

        [SerializeField, ReadOnly]
        private float currentOptionScore = 0f;

        [Header("设置")]
        [SerializeField, Tooltip("决策间隔时间（秒）")]
        private float decisionInterval = 0.5f;

        [SerializeField, Tooltip("是否允许中断当前动作")]
        private bool allowInterrupt = true;

        [SerializeField, Tooltip("中断阈值（新选项分数超过当前选项的倍数）")]
        private float interruptThreshold = 1.2f;

        [SerializeField, Tooltip("选择策略")]
        private SelectionStrategy selectionStrategy = SelectionStrategy.HighestScore;

        [SerializeField, Tooltip("TopN随机选择时的N值")]
        private int topNValue = 3;

        [Header("调试")]
        [SerializeField]
        private bool showDebugInfo = false;

        [ShowIf("showDebugInfo"), ReadOnly]
        [SerializeField]
        private List<DebugOptionInfo> debugOptions = new();

        public UtilityBrain Brain { get; private set; }
        public Blackboard Blackboard => Brain?.Blackboard;

        [System.Serializable]
        private struct DebugOptionInfo
        {
            public string name;
            public float score;
            public bool isCurrent;
        }

        protected virtual void Awake()
        {
            InitializeBrain(null);
        }

        /// <summary>
        /// 初始化大脑
        /// </summary>
        /// <param name="blackboard">可选的共享黑板</param>
        public void InitializeBrain(Blackboard blackboard)
        {
            IOptionSelector selector = selectionStrategy switch
            {
                SelectionStrategy.HighestScore => new HighestScoreSelector(),
                SelectionStrategy.WeightedRandom => new WeightedRandomSelector(),
                SelectionStrategy.TopNRandom => new TopNRandomSelector(topNValue),
                _ => new HighestScoreSelector()
            };

            Brain = new UtilityBrain(blackboard, selector);
            Brain.DecisionInterval = decisionInterval;
            Brain.AllowInterrupt = allowInterrupt;
            Brain.InterruptThreshold = interruptThreshold;

            Brain.OnOptionChanged += OnOptionChanged;

            SetupOptions();
        }

        /// <summary>
        /// 设置选项 - 子类重写此方法添加选项
        /// </summary>
        protected virtual void SetupOptions()
        {
        }

        protected virtual void Start()
        {
            Brain?.Start();
        }

        protected virtual void Update()
        {
            Brain?.Update();
        }

        protected virtual void OnDestroy()
        {
            if (Brain != null)
            {
                Brain.OnOptionChanged -= OnOptionChanged;
            }
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        public void AddOption(Option option)
        {
            Brain?.AddOption(option);
        }

        /// <summary>
        /// 移除选项
        /// </summary>
        public bool RemoveOption(Option option)
        {
            return Brain?.RemoveOption(option) ?? false;
        }

        /// <summary>
        /// 强制做出决策
        /// </summary>
        public void ForceDecision()
        {
            Brain?.ForceDecision();
        }

        private void OnOptionChanged(Option oldOption, Option newOption)
        {
            currentOptionName = newOption?.name ?? "None";
            currentOptionScore = newOption?.LastScore ?? 0f;

            OnOptionChangedCallback(oldOption, newOption);
        }

        /// <summary>
        /// 选项变化回调 - 子类可重写
        /// </summary>
        protected virtual void OnOptionChangedCallback(Option oldOption, Option newOption)
        {
        }
    }
}
