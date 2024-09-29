using UnityEngine;

namespace Utility.Math
{
    public class MathUtil : MonoBehaviour
    {
        /// <summary>
        /// 二次Bezier曲线
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"> 0~1 </param>
        /// <returns></returns>
        public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2,
            float t)
        {
            var t1 = (1 - t) * (1 - t);
            var t2 = t * (1 - t);
            var t3 = t * t;
            return p0 * t1 + 2 * t2 * p1 + t3 * p2;
        }

        public static Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2,
            float t)
        {
            var t1 = (1 - t) * (1 - t);
            var t2 = t * (1 - t);
            var t3 = t * t;
            return p0 * t1 + 2 * t2 * p1 + t3 * p2;
        }

    }
}
