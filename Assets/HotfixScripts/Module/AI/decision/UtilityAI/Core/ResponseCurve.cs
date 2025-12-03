using System;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// 响应曲线类型
    /// </summary>
    public enum CurveType
    {
        /// <summary>
        /// 线性 y = mx + b
        /// </summary>
        Linear,
        /// <summary>
        /// 指数 y = x^power
        /// </summary>
        Exponential,
        /// <summary>
        /// 对数 y = log(x)
        /// </summary>
        Logarithmic,
        /// <summary>
        /// S型曲线 (Logistic)
        /// </summary>
        Sigmoid,
        /// <summary>
        /// 阶梯函数
        /// </summary>
        Step,
        /// <summary>
        /// 自定义AnimationCurve
        /// </summary>
        Custom
    }

    /// <summary>
    /// 响应曲线 - 将输入值映射到0-1的效用分数
    /// 用于将各种游戏数值（如距离、血量）转换为决策权重
    /// </summary>
    [Serializable]
    public class ResponseCurve
    {
        /// <summary>
        /// 曲线类型
        /// </summary>
        public CurveType curveType = CurveType.Linear;

        /// <summary>
        /// 斜率（用于线性曲线）
        /// </summary>
        public float slope = 1f;

        /// <summary>
        /// Y轴偏移
        /// </summary>
        public float yOffset = 0f;

        /// <summary>
        /// X轴偏移
        /// </summary>
        public float xOffset = 0f;

        /// <summary>
        /// 指数（用于指数曲线）
        /// </summary>
        public float exponent = 2f;

        /// <summary>
        /// 自定义曲线
        /// </summary>
        public AnimationCurve customCurve;

        /// <summary>
        /// 是否反转结果
        /// </summary>
        public bool invert = false;

        /// <summary>
        /// 输入值范围的最小值
        /// </summary>
        public float inputMin = 0f;

        /// <summary>
        /// 输入值范围的最大值
        /// </summary>
        public float inputMax = 1f;

        public ResponseCurve()
        {
            customCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        /// <summary>
        /// 计算响应值
        /// </summary>
        /// <param name="input">原始输入值</param>
        /// <returns>0-1之间的效用分数</returns>
        public float Evaluate(float input)
        {
            // 标准化输入到0-1范围
            float normalizedInput = Mathf.InverseLerp(inputMin, inputMax, input);
            normalizedInput = Mathf.Clamp01(normalizedInput);

            float result = curveType switch
            {
                CurveType.Linear => EvaluateLinear(normalizedInput),
                CurveType.Exponential => EvaluateExponential(normalizedInput),
                CurveType.Logarithmic => EvaluateLogarithmic(normalizedInput),
                CurveType.Sigmoid => EvaluateSigmoid(normalizedInput),
                CurveType.Step => EvaluateStep(normalizedInput),
                CurveType.Custom => customCurve?.Evaluate(normalizedInput) ?? normalizedInput,
                _ => normalizedInput
            };

            result = Mathf.Clamp01(result + yOffset);

            if (invert)
            {
                result = 1f - result;
            }

            return result;
        }

        private float EvaluateLinear(float x)
        {
            return slope * (x + xOffset);
        }

        private float EvaluateExponential(float x)
        {
            return Mathf.Pow(x + xOffset, exponent);
        }

        private float EvaluateLogarithmic(float x)
        {
            // 避免log(0)
            float adjustedX = Mathf.Max(0.001f, x + xOffset);
            return Mathf.Log(adjustedX * 9 + 1) / Mathf.Log(10); // 归一化到0-1
        }

        private float EvaluateSigmoid(float x)
        {
            // Logistic函数: 1 / (1 + e^(-k*(x-0.5)))
            float k = slope * 10f; // 控制曲线陡峭程度
            return 1f / (1f + Mathf.Exp(-k * (x + xOffset - 0.5f)));
        }

        private float EvaluateStep(float x)
        {
            return (x + xOffset) >= slope ? 1f : 0f;
        }

        #region 静态工厂方法

        /// <summary>
        /// 创建线性曲线
        /// </summary>
        public static ResponseCurve Linear(float slope = 1f, float inputMin = 0f, float inputMax = 1f)
        {
            return new ResponseCurve
            {
                curveType = CurveType.Linear,
                slope = slope,
                inputMin = inputMin,
                inputMax = inputMax
            };
        }

        /// <summary>
        /// 创建反向线性曲线（值越大，效用越低）
        /// </summary>
        public static ResponseCurve InverseLinear(float inputMin = 0f, float inputMax = 1f)
        {
            return new ResponseCurve
            {
                curveType = CurveType.Linear,
                slope = 1f,
                invert = true,
                inputMin = inputMin,
                inputMax = inputMax
            };
        }

        /// <summary>
        /// 创建指数曲线
        /// </summary>
        public static ResponseCurve Exponential(float power = 2f, float inputMin = 0f, float inputMax = 1f)
        {
            return new ResponseCurve
            {
                curveType = CurveType.Exponential,
                exponent = power,
                inputMin = inputMin,
                inputMax = inputMax
            };
        }

        /// <summary>
        /// 创建S型曲线
        /// </summary>
        public static ResponseCurve Sigmoid(float steepness = 1f, float inputMin = 0f, float inputMax = 1f)
        {
            return new ResponseCurve
            {
                curveType = CurveType.Sigmoid,
                slope = steepness,
                inputMin = inputMin,
                inputMax = inputMax
            };
        }

        #endregion
    }
}
