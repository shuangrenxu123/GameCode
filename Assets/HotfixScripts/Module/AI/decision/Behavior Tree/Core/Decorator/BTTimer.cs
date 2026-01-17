using UnityEngine;

namespace BT
{
    /// <summary>
    /// 延迟修饰器：会在 n 秒后才开始执行子节点，在此之前都会返回Running
    /// 不要试图把他当成一个停止间隔节点用
    /// </summary>
    [BT.EditorIntegration.BTEditorNode("Decorator/Timer", BT.EditorIntegration.BTEditorNodeKind.Decorator)]
    public class BTTimer : BTDecorator
    {
        private float timer = 0;
        public float interval;
        [BT.EditorIntegration.BTEditorConstructor]
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
