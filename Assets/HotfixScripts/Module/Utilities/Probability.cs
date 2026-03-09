using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utility
{
    public class Probability
    {
        /// <summary>
        /// 根据概率获得一个Bool值
        /// </summary>
        /// <param name="PValue">为True的概率值,范围0-100</param>
        /// <returns></returns>
        public static bool GetBool(int PValue)
        {
            if (PValue < 0)
                return false;
            if (PValue > 100)
                return true;
            int p = Random.Range(0, 100);
            return PValue >= p;
        }
        /// <summary>
        /// 轮盘赌算法，返回所对应区间下标
        /// </summary>
        /// <param name="PValues"></param>
        /// <returns></returns>
        public static int RouletteWheel(int[] PValues)
        {
            int sum = PValues.Sum();
            int p = Random.Range(0, sum);
            for (int i = 0; i < PValues.Length; i++)
            {
                if (p <= PValues[i])
                {
                    return i;
                }
                else
                {
                    p -= PValues[i];
                }
            }
            return -1;
        }

        public static void Shuffle<T>(ref T[] pokeArr)
        {
            for (int i = 0; i < pokeArr.Length; i++)
            {
                T temp = pokeArr[i];
                int randomIndex = UnityEngine.Random.Range(0, pokeArr.Length);
                pokeArr[i] = pokeArr[randomIndex];
                pokeArr[randomIndex] = temp;
            }
        }

        /// <summary>
        /// 通过二次贝叶斯概率返回一个bool值
        /// </summary>
        /// <param name="a">A事件发生的次数</param>
        /// <param name="ac">A发生以后C事件发生的次数，该值不可能大于A</param>
        /// <param name="b">B事件发生的次数，该事件与A事件对</param>
        /// <param name="bc">B发生以后C发生的次数，该值不可能大于就B</param>
        /// <returns> C发生的情况下他是不是A事件 </returns>
        public static bool BayesianProbability(int a, int ac, int b, int bc)
        {
            if (ac > a || bc > b)
            {
                throw new Exception("传入的参数有问题");
            }


            int allEventCount = a + b;

            if (allEventCount == 0 || a == 0 || b == 0)
            {
                return true;
            }

            var p1 = (ac / a);
            var p2 = a / allEventCount;

            var probability = (p1 * p2) / (p1 * p2 + (bc / b) * (1 - p2));

            return GetBool(probability * 100);

        }

        /// <summary>
        /// 获取一个符合正态分布（高斯分布）的随机值
        /// </summary>
        /// <param name="mean">均值</param>
        /// <param name="stdDev">标准差，必须大于0</param>
        /// <returns>正态分布随机值</returns>
        public static float NormalDistribution(float mean = 0f, float stdDev = 1f)
        {
            if (stdDev <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(stdDev), "标准差必须大于0");
            }

            float u1 = 1f - Random.value;
            float u2 = 1f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        /// <summary>
        /// 采样一维柏林噪声，返回0~1之间的平滑随机值
        /// </summary>
        /// <param name="x">一维采样位置</param>
        /// <param name="scale">缩放系数，越大越平滑</param>
        /// <param name="offset">采样偏移，可用于区分不同噪声序列</param>
        /// <returns>0~1之间的噪声值</returns>
        public static float PerlinNoise(float x, float scale = 32f, float offset = 0f)
        {
            if (scale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "scale必须大于0");
            }

            float sampleX = (x + offset) / scale;
            return Mathf.PerlinNoise(sampleX, 0f);
        }

        /// <summary>
        /// 采样二维柏林噪声，返回0~1之间的平滑随机值
        /// </summary>
        /// <param name="x">二维采样点X坐标</param>
        /// <param name="y">二维采样点Y坐标</param>
        /// <param name="scale">缩放系数，越大越平滑</param>
        /// <param name="offsetX">X方向偏移量</param>
        /// <param name="offsetY">Y方向偏移量</param>
        /// <returns>0~1之间的噪声值</returns>
        public static float PerlinNoise2D(float x, float y, float scale = 32f)
        {
            if (scale <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "scale必须大于0");
            }

            float sampleX = x / scale;
            float sampleY = y / scale;
            return Mathf.PerlinNoise(sampleX, sampleY);
        }
    }
}
