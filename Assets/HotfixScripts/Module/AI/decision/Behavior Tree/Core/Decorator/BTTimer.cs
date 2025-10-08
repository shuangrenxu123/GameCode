using UnityEngine;

namespace BT
{
    /// <summary>
    /// 延迟修饰器：会在 n 秒后才开始执行子节点，在此之前都会返回Running
    /// </summary>
    public class BTTimer<TKey, TValue> : BTDecorator
    {
        private float timer = 0;
        public float interval;
        public BTTimer(float timer, BTNode child) : base(child)
        {
            interval = timer;
        }

        public override BTResult Tick()
        {
            if (timer > interval)
            {
                timer = 0;
                BTResult result = child.Tick();
                return result;
            }
            else
            {
                timer += Time.deltaTime;
                return BTResult.Running;
            }
        }
        public override void Clear()
        {
            base.Clear();
            timer = 0;
        }
    }
}
