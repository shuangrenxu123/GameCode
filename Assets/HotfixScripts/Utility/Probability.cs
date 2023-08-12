using System.Linq;
using UnityEngine;

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
            return PValue <= p;
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
    }
}
