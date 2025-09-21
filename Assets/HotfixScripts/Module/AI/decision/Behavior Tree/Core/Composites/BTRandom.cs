using System.Collections.Generic;
using System.Linq;
using Utility;

namespace BT
{
    public class BTRandom<TKey, TValue> : BTComposite<TKey, TValue>
    {
        private List<int> probability = new();
        BTNode<TKey, TValue> currentNode;
        public BTComposite<TKey, TValue> AddChild(int prob,BTNode<TKey, TValue> node)
        {
            probability.Add(prob);
            return base.AddChild(node);
        }
        public override void RemoveChild(BTNode<TKey, TValue> node)
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
