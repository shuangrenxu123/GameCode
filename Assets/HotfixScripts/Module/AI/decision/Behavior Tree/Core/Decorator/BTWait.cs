using UnityEngine;

namespace BT
{

    [BT.EditorIntegration.BTEditorNode("Decorator/Wait", EditorIntegration.BTEditorNodeKind.Action)]
    public class BTWait : BTAction
    {
        private float timer = 0;

        [BT.EditorIntegration.BTEditorExpose]
        public float interval;


        public override BTResult Tick()
        {
            if (timer > interval)
            {
                timer = 0;
                return BTResult.Success;
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
