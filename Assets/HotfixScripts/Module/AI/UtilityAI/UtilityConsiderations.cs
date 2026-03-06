using AIBlackboard;
using UnityEngine;

namespace AI.UtilityAI
{
    public sealed class ConstantConsideration : UtilityConsideration
    {
        private readonly float score;

        public ConstantConsideration(Blackboard blackboard, float score) : base(blackboard)
        {
            this.score = score;
        }

        public override float CalculateScore()
        {
            return Mathf.Clamp01(score);
        }
    }

    public sealed class LinearConsideration : UtilityConsideration
    {
        private readonly float slope;
        private readonly float verticalShift;
        private readonly float horizontalShift;

        public LinearConsideration(Blackboard blackboard, BlackboardKey<float> key, float slope, float verticalShift, float horizontalShift)
            : base(blackboard, key)
        {
            this.slope = slope;
            this.verticalShift = verticalShift;
            this.horizontalShift = horizontalShift;
        }

        public override float CalculateScore()
        {
            float score = slope * (value - verticalShift) + horizontalShift;
            return Mathf.Clamp01(score);
        }
    }
}
