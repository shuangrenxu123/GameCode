using System.Collections.Generic;
using System.Linq;
using Utility;

namespace BT
{
    public class BTRandom : BTComposite
    {
        private List<int> probability;
        BTNode currentNode;
        public BTComposite AddChild(int prob,BTNode node)
        {
            probability.Add(prob);
            return base.AddChild(node);
        }
        public override void RemoveChild(BTNode node)
        {
            var index = children.IndexOf(node);
            probability.RemoveAt(index);
            base.RemoveChild(node);
        }
        public override BTResult Tick()
        {

            if (currentNode == null)
            {
                var index = Probability.RouletteWheel(probability.ToArray());
                currentNode = children[index];
            }
            var result = currentNode.Tick();
            if (result != BTResult.Running)
            {
                currentNode = null;
            }
  
            return result;
            
        }
    }
}
