using System.Collections.Generic;
using AIBlackboard;
using UnityEngine;

namespace AI.UtilityAI
{
    public abstract class UtilityNode<T> where T : struct, System.Enum
    {
        public T type;
        public BlackboardKey<float> valueKey;
        protected Blackboard blackboard;
        protected float value => blackboard.GetValue(valueKey);
        public UtilityNode(T type, Blackboard blackboard, BlackboardKey<float> key)
        {
            this.blackboard = blackboard;
            this.type = type;
            this.valueKey = key;
        }
        public abstract float CalculateScore();
        public virtual bool CheckCondition()
        {
            return true;
        }
    }
    public class UtilityBrain<T> where T : struct, System.Enum
    {
        List<UtilityNode<T>> nodes;

        public UtilityBrain()
        {
            nodes = new();
        }

        public void Register(UtilityNode<T> node)
        {
            nodes.Add(node);
        }

        public void UnRegister(UtilityNode<T> node)
        {
            nodes.Remove(node);
        }
        public T Run()
        {
            if (nodes == null || nodes.Count == 0)
            {
                return default;
            }
            UtilityNode<T> temp = nodes[0];
            float score = float.MinValue;
            foreach (var node in nodes)
            {
                if (!node.CheckCondition())
                {
                    continue;
                }
                var v = node.CalculateScore();
                if (v > score)
                {
                    score = v;
                    temp = node;
                }
            }
            return temp.type;
        }
    }
}
