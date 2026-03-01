using System;

namespace Bayes
{
    public sealed class BayesSimpleContext
    {
        private readonly float _smoothing;

        private float _conditionTrueTargetTrueCount;
        private float _conditionTrueTargetFalseCount;
        private float _conditionFalseTargetTrueCount;
        private float _conditionFalseTargetFalseCount;

        private BayesSimpleContext(float smoothing)
        {
            if (smoothing < 0f)
            {
                throw new ArgumentException("平滑系数不能小于 0。", nameof(smoothing));
            }

            _smoothing = smoothing;
        }

        public static BayesSimpleContext Create(float smoothing = 0f)
        {
            return new BayesSimpleContext(smoothing);
        }

        public void Set(
            float conditionTrueTargetTrueCount,
            float conditionTrueTargetFalseCount,
            float conditionFalseTargetTrueCount,
            float conditionFalseTargetFalseCount)
        {
            ValidateCount(conditionTrueTargetTrueCount, nameof(conditionTrueTargetTrueCount));
            ValidateCount(conditionTrueTargetFalseCount, nameof(conditionTrueTargetFalseCount));
            ValidateCount(conditionFalseTargetTrueCount, nameof(conditionFalseTargetTrueCount));
            ValidateCount(conditionFalseTargetFalseCount, nameof(conditionFalseTargetFalseCount));

            _conditionTrueTargetTrueCount = conditionTrueTargetTrueCount;
            _conditionTrueTargetFalseCount = conditionTrueTargetFalseCount;
            _conditionFalseTargetTrueCount = conditionFalseTargetTrueCount;
            _conditionFalseTargetFalseCount = conditionFalseTargetFalseCount;
        }

        public void Update(bool conditionValue, bool targetValue)
        {
            if (conditionValue)
            {
                if (targetValue)
                {
                    _conditionTrueTargetTrueCount += 1f;
                }
                else
                {
                    _conditionTrueTargetFalseCount += 1f;
                }
            }
            else
            {
                if (targetValue)
                {
                    _conditionFalseTargetTrueCount += 1f;
                }
                else
                {
                    _conditionFalseTargetFalseCount += 1f;
                }
            }
        }

        public float GetTargetTrueProb(bool conditionValue)
        {
            float numerator;
            float denominator;

            if (conditionValue)
            {
                numerator = _conditionTrueTargetTrueCount + _smoothing;
                denominator = _conditionTrueTargetTrueCount + _conditionTrueTargetFalseCount + (2f * _smoothing);
            }
            else
            {
                numerator = _conditionFalseTargetTrueCount + _smoothing;
                denominator = _conditionFalseTargetTrueCount + _conditionFalseTargetFalseCount + (2f * _smoothing);
            }

            if (denominator <= 0f)
            {
                throw new InvalidOperationException("当前条件分支样本数为 0，请先调用 Set/Update 或使用 smoothing > 0。");
            }

            return numerator / denominator;
        }

        private static void ValidateCount(float value, string paramName)
        {
            if (value < 0f)
            {
                throw new ArgumentException("计数不能为负数。", paramName);
            }
        }
    }
}
