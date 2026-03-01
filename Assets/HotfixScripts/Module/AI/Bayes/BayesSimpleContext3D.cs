using System;

namespace Bayes
{
    public sealed class BayesSimpleContext3D
    {
        private readonly float _smoothing;

        private float _aTrueBTrueTargetTrueCount;
        private float _aTrueBTrueTargetFalseCount;
        private float _aTrueBFalseTargetTrueCount;
        private float _aTrueBFalseTargetFalseCount;
        private float _aFalseBTrueTargetTrueCount;
        private float _aFalseBTrueTargetFalseCount;
        private float _aFalseBFalseTargetTrueCount;
        private float _aFalseBFalseTargetFalseCount;

        private BayesSimpleContext3D(float smoothing)
        {
            if (smoothing < 0f)
            {
                throw new ArgumentException("Smoothing must be >= 0.", nameof(smoothing));
            }

            _smoothing = smoothing;
        }

        public static BayesSimpleContext3D Create(float smoothing = 0f)
        {
            return new BayesSimpleContext3D(smoothing);
        }

        public void Set(
            float conditionATrueConditionBTrueTargetTrueCount,
            float conditionATrueConditionBTrueTargetFalseCount,
            float conditionATrueConditionBFalseTargetTrueCount,
            float conditionATrueConditionBFalseTargetFalseCount,
            float conditionAFalseConditionBTrueTargetTrueCount,
            float conditionAFalseConditionBTrueTargetFalseCount,
            float conditionAFalseConditionBFalseTargetTrueCount,
            float conditionAFalseConditionBFalseTargetFalseCount)
        {
            ValidateCount(conditionATrueConditionBTrueTargetTrueCount, nameof(conditionATrueConditionBTrueTargetTrueCount));
            ValidateCount(conditionATrueConditionBTrueTargetFalseCount, nameof(conditionATrueConditionBTrueTargetFalseCount));
            ValidateCount(conditionATrueConditionBFalseTargetTrueCount, nameof(conditionATrueConditionBFalseTargetTrueCount));
            ValidateCount(conditionATrueConditionBFalseTargetFalseCount, nameof(conditionATrueConditionBFalseTargetFalseCount));
            ValidateCount(conditionAFalseConditionBTrueTargetTrueCount, nameof(conditionAFalseConditionBTrueTargetTrueCount));
            ValidateCount(conditionAFalseConditionBTrueTargetFalseCount, nameof(conditionAFalseConditionBTrueTargetFalseCount));
            ValidateCount(conditionAFalseConditionBFalseTargetTrueCount, nameof(conditionAFalseConditionBFalseTargetTrueCount));
            ValidateCount(conditionAFalseConditionBFalseTargetFalseCount, nameof(conditionAFalseConditionBFalseTargetFalseCount));

            _aTrueBTrueTargetTrueCount = conditionATrueConditionBTrueTargetTrueCount;
            _aTrueBTrueTargetFalseCount = conditionATrueConditionBTrueTargetFalseCount;
            _aTrueBFalseTargetTrueCount = conditionATrueConditionBFalseTargetTrueCount;
            _aTrueBFalseTargetFalseCount = conditionATrueConditionBFalseTargetFalseCount;
            _aFalseBTrueTargetTrueCount = conditionAFalseConditionBTrueTargetTrueCount;
            _aFalseBTrueTargetFalseCount = conditionAFalseConditionBTrueTargetFalseCount;
            _aFalseBFalseTargetTrueCount = conditionAFalseConditionBFalseTargetTrueCount;
            _aFalseBFalseTargetFalseCount = conditionAFalseConditionBFalseTargetFalseCount;
        }

        public void Update(bool conditionAValue, bool conditionBValue, bool targetValue)
        {
            if (conditionAValue)
            {
                if (conditionBValue)
                {
                    if (targetValue)
                    {
                        _aTrueBTrueTargetTrueCount += 1f;
                    }
                    else
                    {
                        _aTrueBTrueTargetFalseCount += 1f;
                    }
                }
                else
                {
                    if (targetValue)
                    {
                        _aTrueBFalseTargetTrueCount += 1f;
                    }
                    else
                    {
                        _aTrueBFalseTargetFalseCount += 1f;
                    }
                }
            }
            else
            {
                if (conditionBValue)
                {
                    if (targetValue)
                    {
                        _aFalseBTrueTargetTrueCount += 1f;
                    }
                    else
                    {
                        _aFalseBTrueTargetFalseCount += 1f;
                    }
                }
                else
                {
                    if (targetValue)
                    {
                        _aFalseBFalseTargetTrueCount += 1f;
                    }
                    else
                    {
                        _aFalseBFalseTargetFalseCount += 1f;
                    }
                }
            }
        }

        public float GetTargetTrueProb(bool conditionAValue, bool conditionBValue)
        {
            float trueCount;
            float falseCount;

            if (conditionAValue)
            {
                if (conditionBValue)
                {
                    trueCount = _aTrueBTrueTargetTrueCount;
                    falseCount = _aTrueBTrueTargetFalseCount;
                }
                else
                {
                    trueCount = _aTrueBFalseTargetTrueCount;
                    falseCount = _aTrueBFalseTargetFalseCount;
                }
            }
            else
            {
                if (conditionBValue)
                {
                    trueCount = _aFalseBTrueTargetTrueCount;
                    falseCount = _aFalseBTrueTargetFalseCount;
                }
                else
                {
                    trueCount = _aFalseBFalseTargetTrueCount;
                    falseCount = _aFalseBFalseTargetFalseCount;
                }
            }

            float numerator = trueCount + _smoothing;
            float denominator = trueCount + falseCount + (2f * _smoothing);
            if (denominator <= 0f)
            {
                throw new InvalidOperationException("Current branch has no samples. Call Set/Update first or use smoothing > 0.");
            }

            return numerator / denominator;
        }

        private static void ValidateCount(float value, string paramName)
        {
            if (value < 0f)
            {
                throw new ArgumentException("Count cannot be negative.", paramName);
            }
        }
    }
}
