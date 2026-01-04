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
        public static bool BayesianProbability(int a ,int ac,int b,int bc)
        {
            if(ac > a || bc  > b)
            {
                throw new Exception("传入的参数有问题");
            }
        

            int allEventCount = a + b;

            if(allEventCount == 0 || a == 0 || b == 0){
                return true;
            }

            var p1 = (ac / a);
            var p2 = a/ allEventCount;
            
            var probability = (p1 * p2 ) / (p1* p2 + (bc / b) * (1 - p2));

            return GetBool(probability * 100);
            
        }
    }
}
