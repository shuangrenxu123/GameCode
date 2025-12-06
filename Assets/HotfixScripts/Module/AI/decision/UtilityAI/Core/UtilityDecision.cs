using System;
using System.Collections.Generic;

namespace UtilityAI
{
    /// <summary>
    /// 效用AI决策结果
    /// </summary>
    [Serializable]
    public struct UtilityDecision
    {
        /// <summary>
        /// 选中的选项名称
        /// </summary>
        public string OptionName;

        /// <summary>
        /// 选中的选项分数
        /// </summary>
        public float Score;

        /// <summary>
        /// 决策时间戳
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// 决策是否有效（是否有选中的选项）
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(OptionName);

        public static UtilityDecision Invalid => new UtilityDecision { OptionName = null, Score = 0f, Timestamp = 0f };

        public override string ToString()
        {
            return IsValid ? $"{OptionName} ({Score:F2})" : "None";
        }
    }
}
