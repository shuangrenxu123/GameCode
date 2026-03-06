using System.Collections.Generic;
using AIBlackboard;

namespace AI.UtilityAI
{
    public readonly struct UtilityDecisionResult<T> where T : struct, System.Enum
    {
        public readonly T type;
        public readonly float score;
        public readonly bool hasDecision;
        public readonly bool isChanged;

        public UtilityDecisionResult(T type, float score, bool hasDecision, bool isChanged)
        {
            this.type = type;
            this.score = score;
            this.hasDecision = hasDecision;
            this.isChanged = isChanged;
        }
    }

    public enum UtilityScoreAggregation
    {
        Multiply,
        Sum
    }

    public abstract class UtilityConsideration
    {
        public BlackboardKey<float> valueKey;
        protected readonly Blackboard blackboard;
        protected float value => blackboard.GetValue(valueKey);
        protected UtilityConsideration(Blackboard blackboard, BlackboardKey<float> valueKey)
        {
            this.valueKey = valueKey;
            this.blackboard = blackboard;
        }
        protected UtilityConsideration(Blackboard blackboard)
        {
            this.blackboard = blackboard;
            valueKey = default;
        }

        public virtual bool CheckCondition()
        {
            return true;
        }

        public abstract float CalculateScore();
    }

    public abstract class UtilityNode<T> where T : struct, System.Enum
    {
        public T type;

        protected Blackboard blackboard;

        protected UtilityNode(T type1, Blackboard blackboard)
        {
            this.type = type1;
            this.blackboard = blackboard;
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
        T lastNodeType;
        float lastScore;
        bool hasLastDecision;

        public UtilityBrain()
        {
            nodes = new List<UtilityNode<T>>();
        }

        public void Register(UtilityNode<T> node)
        {
            if (node == null || nodes.Contains(node))
            {
                return;
            }

            nodes.Add(node);
        }

        public void UnRegister(UtilityNode<T> node)
        {
            if (!nodes.Contains(node))
            {
                return;
            }

            nodes.Remove(node);
        }

        public T Run()
        {
            return Decide().type;
        }

        public UtilityDecisionResult<T> Decide()
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new UtilityDecisionResult<T>(lastNodeType, lastScore, hasLastDecision, false);
            }

            UtilityNode<T> bestNode = null;
            float bestScore = float.MinValue;

            foreach (var node in nodes)
            {
                if (!node.CheckCondition())
                {
                    continue;
                }

                float nodeScore = node.CalculateScore();
                if (nodeScore > bestScore)
                {
                    bestScore = nodeScore;
                    bestNode = node;
                }
            }

            if (bestNode == null)
            {
                return new UtilityDecisionResult<T>(lastNodeType, lastScore, hasLastDecision, false);
            }

            bool isChanged = !hasLastDecision || !EqualityComparer<T>.Default.Equals(lastNodeType, bestNode.type);
            lastNodeType = bestNode.type;
            lastScore = bestScore;
            hasLastDecision = true;

            return new UtilityDecisionResult<T>(lastNodeType, lastScore, true, isChanged);
        }
    }

}
