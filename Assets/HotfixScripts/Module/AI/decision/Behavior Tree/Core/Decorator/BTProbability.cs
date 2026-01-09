using UnityEngine;
using Utility;

namespace BT
{

    [BT.EditorIntegration.BTEditorNode("Decorator/Probability", BT.EditorIntegration.BTEditorNodeKind.Decorator)]
    public class BTProbability: BTDecorator
    {
        int probability;
        BTActionStatus state = BTActionStatus.Ready;
        [BT.EditorIntegration.BTEditorConstructor]
        public BTProbability(int Probability, BTNode child) : base(child)
        {
            this.probability = Probability;
        }
        public override BTResult Tick()
        {
            if (state == BTActionStatus.Running)
            {
                var result = child.Tick();
                if (result != BTResult.Running)
                {
                    state = BTActionStatus.Ready;
                }
                return result;
            }
            else
            {
                var prob = Probability.GetBool(probability);
                if (prob)
                {
                    state = BTActionStatus.Running;
                    return child.Tick();

                }
                else
                {
                    return BTResult.Success;
                }

            }
        }
    }
}
