using System;
using System.Collections.Generic;

namespace Bayes
{
    /// <summary>
    /// 面向业务的轻量封装：聚焦二值变量(T/F)的建模与查询。
    /// </summary>
    public sealed class BayesSimpleContext
    {
        private static readonly string[] BoolValues = { bool.TrueString, bool.FalseString };
        private readonly BayesContext _ctx;

        private BayesSimpleContext(BayesContext ctx)
        {
            _ctx = ctx;
        }

        public static BayesSimpleContext Create()
        {
            return new BayesSimpleContext(BayesContext.Create());
        }

        /// <summary>
        /// 定义一个二值变量，取值固定为 T/F。
        /// </summary>
        public void DefineBoolVar(string name)
        {
            _ctx.DefineVariable(name, BoolValues);
        }

        /// <summary>
        /// 用计数设置分布。无 parentVar 时等价于设置先验 P(var)。
        /// </summary>
        public void SetByCount(string varName, float trueCount, float falseCount, float smoothing = 0f)
        {
            ValidateCounts(trueCount, falseCount, smoothing);
            float t = trueCount + smoothing;
            float f = falseCount + smoothing;
            _ctx.SetPrior(varName, new Distribution(new Dictionary<string, float>
            {
                [Key(true)] = t,
                [Key(false)] = f
            }));
        }

        /// <summary>
        /// 用二维计数设置条件分布 P(var | parentVar)。
        /// </summary>
        public void SetByCount(
            string varName,
            string parentVar,
            float childTrueWhenParentTrue,
            float childFalseWhenParentTrue,
            float childTrueWhenParentFalse,
            float childFalseWhenParentFalse,
            float smoothing = 0f)
        {
            ValidateCounts(childTrueWhenParentTrue, childFalseWhenParentTrue, smoothing);
            ValidateCounts(childTrueWhenParentFalse, childFalseWhenParentFalse, smoothing);

            var cpt = new Cpt(new Dictionary<ParentsKey, Distribution>
            {
                [ParentsKey.FromPairs((parentVar, Key(true)))] = new Distribution(new Dictionary<string, float>
                {
                    [Key(true)] = childTrueWhenParentTrue + smoothing,
                    [Key(false)] = childFalseWhenParentTrue + smoothing
                }),
                [ParentsKey.FromPairs((parentVar, Key(false)))] = new Distribution(new Dictionary<string, float>
                {
                    [Key(true)] = childTrueWhenParentFalse + smoothing,
                    [Key(false)] = childFalseWhenParentFalse + smoothing
                })
            });

            _ctx.SetConditional(varName, new[] { parentVar }, cpt);
        }

        /// <summary>
        /// 一次调用同时设置 P(parentVar) 与 P(var | parentVar)。
        /// </summary>
        public void SetByCount(
            string varName,
            string parentVar,
            float parentTrueCount,
            float parentFalseCount,
            float childTrueWhenParentTrue,
            float childFalseWhenParentTrue,
            float childTrueWhenParentFalse,
            float childFalseWhenParentFalse,
            float smoothing = 0f)
        {
            SetByCount(parentVar, parentTrueCount, parentFalseCount, smoothing);
            SetByCount(
                varName,
                parentVar,
                childTrueWhenParentTrue,
                childFalseWhenParentTrue,
                childTrueWhenParentFalse,
                childFalseWhenParentFalse,
                smoothing);
        }

        /// <summary>
        /// 用三维计数设置条件分布 P(var | parentA, parentB)。
        /// 组合顺序固定为: TT, TF, FT, FF（对应 parentA/parentB）。
        /// </summary>
        public void SetByCount(
            string varName,
            string parentA,
            string parentB,
            float childTrueWhenTT,
            float childFalseWhenTT,
            float childTrueWhenTF,
            float childFalseWhenTF,
            float childTrueWhenFT,
            float childFalseWhenFT,
            float childTrueWhenFF,
            float childFalseWhenFF,
            float smoothing = 0f)
        {
            ValidateCounts(childTrueWhenTT, childFalseWhenTT, smoothing);
            ValidateCounts(childTrueWhenTF, childFalseWhenTF, smoothing);
            ValidateCounts(childTrueWhenFT, childFalseWhenFT, smoothing);
            ValidateCounts(childTrueWhenFF, childFalseWhenFF, smoothing);

            var cpt = new Cpt(new Dictionary<ParentsKey, Distribution>
            {
                [ParentsKey.FromPairs((parentA, Key(true)), (parentB, Key(true)))] = BuildBoolDistribution(childTrueWhenTT, childFalseWhenTT, smoothing),
                [ParentsKey.FromPairs((parentA, Key(true)), (parentB, Key(false)))] = BuildBoolDistribution(childTrueWhenTF, childFalseWhenTF, smoothing),
                [ParentsKey.FromPairs((parentA, Key(false)), (parentB, Key(true)))] = BuildBoolDistribution(childTrueWhenFT, childFalseWhenFT, smoothing),
                [ParentsKey.FromPairs((parentA, Key(false)), (parentB, Key(false)))] = BuildBoolDistribution(childTrueWhenFF, childFalseWhenFF, smoothing)
            });

            _ctx.SetConditional(varName, new[] { parentA, parentB }, cpt);
        }

        /// <summary>
        /// 查询 P(target=T | evidence)。
        /// </summary>
        public float PosteriorTrue(string targetVar, params BoolEvidence[] evidence)
        {
            return _ctx.Probability(targetVar, Key(true), BuildEvidenceSet(evidence));
        }

        /// <summary>
        /// 查询 P(target=F | evidence)。
        /// </summary>
        public float PosteriorFalse(string targetVar, params BoolEvidence[] evidence)
        {
            return _ctx.Probability(targetVar, Key(false), BuildEvidenceSet(evidence));
        }

        /// <summary>
        /// 暴露底层上下文，保留高级能力。
        /// </summary>
        public BayesContext Raw => _ctx;

        private static void ValidateCounts(float a, float b, float smoothing)
        {
            if (a < 0f || b < 0f)
            {
                throw new ArgumentException("计数不能为负数。");
            }
            if (smoothing < 0f)
            {
                throw new ArgumentException("平滑系数不能小于 0。", nameof(smoothing));
            }
            if ((a + b + (2f * smoothing)) <= 0f)
            {
                throw new ArgumentException("分布总计数必须大于 0。");
            }
        }

        private static EvidenceSet BuildEvidenceSet(IReadOnlyList<BoolEvidence> evidence)
        {
            if (evidence == null || evidence.Count == 0)
            {
                return EvidenceSet.Create();
            }

            var items = new Evidence[evidence.Count];
            for (int i = 0; i < evidence.Count; i++)
            {
                items[i] = Evidence.Of(evidence[i].Var, ToToken(evidence[i].Value));
            }

            return EvidenceSet.Create(items);
        }

        private static Distribution BuildBoolDistribution(float trueCount, float falseCount, float smoothing)
        {
            return new Distribution(new Dictionary<string, float>
            {
                [Key(true)] = trueCount + smoothing,
                [Key(false)] = falseCount + smoothing
            });
        }

        private static string ToToken(bool value)
        {
            return Key(value);
        }

        private static string Key(bool value)
        {
            return value ? bool.TrueString : bool.FalseString;
        }
    }

    public readonly struct BoolEvidence
    {
        public string Var { get; }
        public bool Value { get; }

        public BoolEvidence(string var, bool value)
        {
            Var = var ?? throw new ArgumentNullException(nameof(var));
            Value = value;
        }

        public static BoolEvidence Of(string var, bool value)
        {
            return new BoolEvidence(var, value);
        }
    }
}
