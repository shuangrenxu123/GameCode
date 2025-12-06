using AIBlackboard;
using Fight;
using UnityEngine;
using UtilityAI;

namespace Enemy.AI.Utility
{
    /// <summary>
    /// 根据血量百分比计算权重，血量越低得分越高
    /// </summary>
    public class LowHealthConsideration : IConsideration
    {
        public string Name { get; }
        readonly BlackboardKey<CombatEntity> combatEntityKey;
        readonly BlackboardKey<float> thresholdKey;

        public LowHealthConsideration(string name, BlackboardKey<CombatEntity> combatEntityKey, BlackboardKey<float> thresholdKey)
        {
            Name = name;
            this.combatEntityKey = combatEntityKey;
            this.thresholdKey = thresholdKey;
        }

        public float Evaluate(Blackboard blackboard)
        {
            var combatEntity = blackboard.GetValue(combatEntityKey, null);
            if (combatEntity?.hp == null)
            {
                return 0f;
            }

            float threshold = Mathf.Clamp01(blackboard.GetValue(thresholdKey, 0.3f));
            float percent = Mathf.Clamp01(combatEntity.hp.Percent);
            if (percent >= threshold || threshold <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01((threshold - percent) / Mathf.Max(threshold, 0.0001f));
        }
    }

    /// <summary>
    /// 根据血量百分比计算权重，血量越高得分越高
    /// </summary>
    public class HighHealthConsideration : IConsideration
    {
        public string Name { get; }
        readonly BlackboardKey<CombatEntity> combatEntityKey;
        readonly BlackboardKey<float> thresholdKey;

        public HighHealthConsideration(string name, BlackboardKey<CombatEntity> combatEntityKey, BlackboardKey<float> thresholdKey)
        {
            Name = name;
            this.combatEntityKey = combatEntityKey;
            this.thresholdKey = thresholdKey;
        }

        public float Evaluate(Blackboard blackboard)
        {
            var combatEntity = blackboard.GetValue(combatEntityKey, null);
            if (combatEntity?.hp == null)
            {
                return 0f;
            }

            float threshold = Mathf.Clamp01(blackboard.GetValue(thresholdKey, 0.3f));
            float percent = Mathf.Clamp01(combatEntity.hp.Percent);
            if (percent <= threshold)
            {
                return Mathf.Clamp01(percent / Mathf.Max(threshold, 0.0001f));
            }

            return 1f;
        }
    }

    /// <summary>
    /// 基于与玩家距离的考量，可配置为近距离或远距离优先
    /// </summary>
    public class PlayerDistanceConsideration : IConsideration
    {
        public string Name { get; }
        readonly BlackboardKey<float> distanceKey;
        readonly float referenceDistance;
        readonly bool invert;

        public PlayerDistanceConsideration(string name, BlackboardKey<float> distanceKey, float referenceDistance, bool invert)
        {
            Name = name;
            this.distanceKey = distanceKey;
            this.referenceDistance = Mathf.Max(0.1f, referenceDistance);
            this.invert = invert;
        }

        public float Evaluate(Blackboard blackboard)
        {
            float distance = blackboard.GetValue(distanceKey, float.PositiveInfinity);
            float normalized = Mathf.Clamp01(distance / referenceDistance);
            return invert ? 1f - normalized : normalized;
        }
    }

    /// <summary>
    /// 基于CombatEntity血量百分比的通用考量，使用ResponseCurve
    /// </summary>
    public class CombatEntityHealthConsideration : IConsideration
    {
        public string Name { get; }
        readonly BlackboardKey<CombatEntity> combatEntityKey;
        readonly ResponseCurve curve;

        public CombatEntityHealthConsideration(string name, BlackboardKey<CombatEntity> combatEntityKey, ResponseCurve curve)
        {
            Name = name;
            this.combatEntityKey = combatEntityKey;
            this.curve = curve;
        }

        public float Evaluate(Blackboard blackboard)
        {
            var combatEntity = blackboard.GetValue(combatEntityKey, null);
            if (combatEntity?.hp == null)
            {
                return 0f;
            }

            float percent = Mathf.Clamp01(combatEntity.hp.Percent);
            return curve.Evaluate(percent);
        }
    }
}
