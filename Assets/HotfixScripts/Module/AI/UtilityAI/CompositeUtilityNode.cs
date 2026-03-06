using System.Collections.Generic;
using AIBlackboard;

namespace AI.UtilityAI
{
    public sealed class CompositeUtilityNode<T> : UtilityNode<T> where T : struct, System.Enum
    {
        private readonly List<UtilityConsideration> considerations;
        private readonly UtilityScoreAggregation aggregationMode;

        public CompositeUtilityNode(T type, Blackboard blackboard, UtilityScoreAggregation aggregationMode = UtilityScoreAggregation.Multiply)
            : base(type, blackboard)
        {
            this.aggregationMode = aggregationMode;
            considerations = new List<UtilityConsideration>();
        }

        public CompositeUtilityNode<T> AddConsideration(UtilityConsideration consideration)
        {
            if (consideration != null)
            {
                considerations.Add(consideration);
            }

            return this;
        }

        public override float CalculateScore()
        {
            if (considerations.Count == 0)
            {
                return 0f;
            }

            float totalScore = aggregationMode == UtilityScoreAggregation.Multiply ? 1f : 0f;

            foreach (var consideration in considerations)
            {
                if (!consideration.CheckCondition())
                {
                    return 0f;
                }

                float score = consideration.CalculateScore();

                if (aggregationMode == UtilityScoreAggregation.Multiply)
                {
                    totalScore *= score;
                }
                else
                {
                    totalScore += score;
                }
            }

            return totalScore;
        }
    }
}
