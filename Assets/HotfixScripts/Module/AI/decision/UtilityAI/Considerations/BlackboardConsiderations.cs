using System;
using AIBlackboard;

namespace UtilityAI
{
    /// <summary>
    /// 基于黑板数据的考量 - 从Blackboard读取数据并应用响应曲线
    /// </summary>
    /// <typeparam name="T">黑板数据类型</typeparam>
    public class BlackboardConsideration<T> : IConsideration where T : struct
    {
        public string Name { get; }

        private readonly BlackboardKey<T> key;
        private readonly Func<T, float> valueExtractor;
        private readonly ResponseCurve curve;

        /// <summary>
        /// 创建基于黑板的考量
        /// </summary>
        /// <param name="name">考量名称</param>
        /// <param name="key">黑板键</param>
        /// <param name="valueExtractor">将黑板值转换为float的函数</param>
        /// <param name="curve">响应曲线</param>
        public BlackboardConsideration(
            string name,
            BlackboardKey<T> key,
            Func<T, float> valueExtractor,
            ResponseCurve curve = null)
        {
            Name = name;
            this.key = key;
            this.valueExtractor = valueExtractor;
            this.curve = curve ?? ResponseCurve.Linear();
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(key, out T value))
            {
                return 0f;
            }

            float rawValue = valueExtractor(value);
            return curve.Evaluate(rawValue);
        }
    }

    /// <summary>
    /// Float类型黑板考量的快捷方式
    /// </summary>
    public class FloatConsideration : BlackboardConsideration<float>
    {
        public FloatConsideration(string name, BlackboardKey<float> key, ResponseCurve curve = null)
            : base(name, key, v => v, curve)
        {
        }
    }

    /// <summary>
    /// 布尔类型考量 - 检查黑板中的布尔值
    /// </summary>
    public class BoolConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<bool> key;
        private readonly bool expectedValue;
        private readonly float trueScore;
        private readonly float falseScore;

        public BoolConsideration(
            string name,
            BlackboardKey<bool> key,
            bool expectedValue = true,
            float trueScore = 1f,
            float falseScore = 0f)
        {
            Name = name;
            this.key = key;
            this.expectedValue = expectedValue;
            this.trueScore = trueScore;
            this.falseScore = falseScore;
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(key, out bool value))
            {
                return falseScore;
            }

            return (value == expectedValue) ? trueScore : falseScore;
        }
    }

    /// <summary>
    /// 存在性检查考量 - 检查黑板中是否存在某个键
    /// </summary>
    public class ExistsConsideration<T> : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<T> key;
        private readonly float existsScore;
        private readonly float notExistsScore;

        public ExistsConsideration(
            string name,
            BlackboardKey<T> key,
            float existsScore = 1f,
            float notExistsScore = 0f)
        {
            Name = name;
            this.key = key;
            this.existsScore = existsScore;
            this.notExistsScore = notExistsScore;
        }

        public float Evaluate(Blackboard blackboard)
        {
            return blackboard.ContainsData(key) ? existsScore : notExistsScore;
        }
    }

    /// <summary>
    /// 固定值考量 - 始终返回固定分数
    /// </summary>
    public class ConstantConsideration : IConsideration
    {
        public string Name { get; }
        public float Value { get; set; }

        public ConstantConsideration(string name, float value)
        {
            Name = name;
            Value = value;
        }

        public float Evaluate(Blackboard blackboard) => Value;
    }

    /// <summary>
    /// 自定义函数考量
    /// </summary>
    public class FuncConsideration : IConsideration
    {
        public string Name { get; }

        private readonly Func<Blackboard, float> evaluator;

        public FuncConsideration(string name, Func<Blackboard, float> evaluator)
        {
            Name = name;
            this.evaluator = evaluator;
        }

        public float Evaluate(Blackboard blackboard)
        {
            return evaluator?.Invoke(blackboard) ?? 0f;
        }
    }

    /// <summary>
    /// 比较考量 - 比较两个黑板值
    /// </summary>
    public class CompareConsideration<T> : IConsideration where T : IComparable<T>
    {
        public enum CompareOperator
        {
            LessThan,
            LessOrEqual,
            Equal,
            GreaterOrEqual,
            GreaterThan,
            NotEqual
        }

        public string Name { get; }

        private readonly BlackboardKey<T> leftKey;
        private readonly BlackboardKey<T> rightKey;
        private readonly CompareOperator op;
        private readonly T rightConstant;
        private readonly bool useConstant;

        public CompareConsideration(
            string name,
            BlackboardKey<T> leftKey,
            CompareOperator op,
            BlackboardKey<T> rightKey)
        {
            Name = name;
            this.leftKey = leftKey;
            this.op = op;
            this.rightKey = rightKey;
            useConstant = false;
        }

        public CompareConsideration(
            string name,
            BlackboardKey<T> leftKey,
            CompareOperator op,
            T rightValue)
        {
            Name = name;
            this.leftKey = leftKey;
            this.op = op;
            rightConstant = rightValue;
            useConstant = true;
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(leftKey, out T leftValue))
                return 0f;

            T rightValue;
            if (useConstant)
            {
                rightValue = rightConstant;
            }
            else if (!blackboard.TryGetValue(rightKey, out rightValue))
            {
                return 0f;
            }

            int comparison = leftValue.CompareTo(rightValue);

            bool result = op switch
            {
                CompareOperator.LessThan => comparison < 0,
                CompareOperator.LessOrEqual => comparison <= 0,
                CompareOperator.Equal => comparison == 0,
                CompareOperator.GreaterOrEqual => comparison >= 0,
                CompareOperator.GreaterThan => comparison > 0,
                CompareOperator.NotEqual => comparison != 0,
                _ => false
            };

            return result ? 1f : 0f;
        }
    }
}
