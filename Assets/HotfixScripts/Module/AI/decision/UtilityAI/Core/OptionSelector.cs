using System.Collections.Generic;
using AIBlackboard;

namespace UtilityAI
{
    /// <summary>
    /// 选项选择策略
    /// </summary>
    public enum SelectionStrategy
    {
        /// <summary>
        /// 选择分数最高的选项
        /// </summary>
        HighestScore,
        /// <summary>
        /// 根据分数加权随机选择
        /// </summary>
        WeightedRandom,
        /// <summary>
        /// 只考虑前N个高分选项，然后随机选择
        /// </summary>
        TopNRandom
    }

    /// <summary>
    /// 选项选择器接口
    /// </summary>
    public interface IOptionSelector
    {
        /// <summary>
        /// 从候选选项中选择一个
        /// </summary>
        Option Select(List<Option> options, Blackboard blackboard);
    }

    /// <summary>
    /// 最高分选择器
    /// </summary>
    public class HighestScoreSelector : IOptionSelector
    {
        public Option Select(List<Option> options, Blackboard blackboard)
        {
            Option best = null;
            float bestScore = float.MinValue;

            foreach (var option in options)
            {
                float score = option.Evaluate(blackboard);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = option;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// 加权随机选择器
    /// </summary>
    public class WeightedRandomSelector : IOptionSelector
    {
        public Option Select(List<Option> options, Blackboard blackboard)
        {
            float totalScore = 0f;
            foreach (var option in options)
            {
                option.Evaluate(blackboard);
                totalScore += option.LastScore;
            }

            if (totalScore <= 0f) return null;

            float randomValue = UnityEngine.Random.Range(0f, totalScore);
            float cumulative = 0f;

            foreach (var option in options)
            {
                cumulative += option.LastScore;
                if (randomValue <= cumulative)
                {
                    return option;
                }
            }

            return options.Count > 0 ? options[options.Count - 1] : null;
        }
    }

    /// <summary>
    /// 前N加权随机选择器
    /// </summary>
    public class TopNRandomSelector : IOptionSelector
    {
        private readonly int topN;

        public TopNRandomSelector(int n = 3)
        {
            topN = n;
        }

        public Option Select(List<Option> options, Blackboard blackboard)
        {
            // 评估所有选项并排序
            var scoredOptions = new List<(Option option, float score)>();
            foreach (var option in options)
            {
                float score = option.Evaluate(blackboard);
                if (score > 0f)
                {
                    scoredOptions.Add((option, score));
                }
            }

            if (scoredOptions.Count == 0) return null;

            // 按分数降序排序
            scoredOptions.Sort((a, b) => b.score.CompareTo(a.score));

            // 取前N个
            int count = System.Math.Min(topN, scoredOptions.Count);
            float totalScore = 0f;
            for (int i = 0; i < count; i++)
            {
                totalScore += scoredOptions[i].score;
            }

            // 加权随机选择
            float randomValue = UnityEngine.Random.Range(0f, totalScore);
            float cumulative = 0f;

            for (int i = 0; i < count; i++)
            {
                cumulative += scoredOptions[i].score;
                if (randomValue <= cumulative)
                {
                    return scoredOptions[i].option;
                }
            }

            return scoredOptions[0].option;
        }
    }
}
