using AIBlackboard;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// 距离考量 - 基于目标距离计算效用
    /// </summary>
    public class DistanceConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<Vector3> selfPositionKey;
        private readonly BlackboardKey<Vector3> targetPositionKey;
        private readonly ResponseCurve curve;

        public DistanceConsideration(
            string name,
            BlackboardKey<Vector3> selfPositionKey,
            BlackboardKey<Vector3> targetPositionKey,
            ResponseCurve curve = null)
        {
            Name = name;
            this.selfPositionKey = selfPositionKey;
            this.targetPositionKey = targetPositionKey;
            this.curve = curve ?? ResponseCurve.InverseLinear(0f, 20f);
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(selfPositionKey, out Vector3 selfPos) ||
                !blackboard.TryGetValue(targetPositionKey, out Vector3 targetPos))
            {
                return 0f;
            }

            float distance = Vector3.Distance(selfPos, targetPos);
            return curve.Evaluate(distance);
        }
    }

    /// <summary>
    /// Transform距离考量
    /// </summary>
    public class TransformDistanceConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<Transform> selfTransformKey;
        private readonly BlackboardKey<Transform> targetTransformKey;
        private readonly ResponseCurve curve;

        public TransformDistanceConsideration(
            string name,
            BlackboardKey<Transform> selfTransformKey,
            BlackboardKey<Transform> targetTransformKey,
            ResponseCurve curve = null)
        {
            Name = name;
            this.selfTransformKey = selfTransformKey;
            this.targetTransformKey = targetTransformKey;
            this.curve = curve ?? ResponseCurve.InverseLinear(0f, 20f);
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(selfTransformKey, out Transform self) ||
                !blackboard.TryGetValue(targetTransformKey, out Transform target))
            {
                return 0f;
            }

            if (self == null || target == null) return 0f;

            float distance = Vector3.Distance(self.position, target.position);
            return curve.Evaluate(distance);
        }
    }

    /// <summary>
    /// 角度考量 - 目标是否在视野范围内
    /// </summary>
    public class AngleConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<Transform> selfTransformKey;
        private readonly BlackboardKey<Vector3> targetPositionKey;
        private readonly float maxAngle;
        private readonly ResponseCurve curve;

        public AngleConsideration(
            string name,
            BlackboardKey<Transform> selfTransformKey,
            BlackboardKey<Vector3> targetPositionKey,
            float maxAngle = 90f,
            ResponseCurve curve = null)
        {
            Name = name;
            this.selfTransformKey = selfTransformKey;
            this.targetPositionKey = targetPositionKey;
            this.maxAngle = maxAngle;
            this.curve = curve ?? ResponseCurve.InverseLinear(0f, maxAngle);
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(selfTransformKey, out Transform self) ||
                !blackboard.TryGetValue(targetPositionKey, out Vector3 targetPos))
            {
                return 0f;
            }

            if (self == null) return 0f;

            Vector3 directionToTarget = (targetPos - self.position).normalized;
            float angle = Vector3.Angle(self.forward, directionToTarget);

            return curve.Evaluate(angle);
        }
    }

    /// <summary>
    /// 生命值考量
    /// </summary>
    public class HealthConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<float> currentHealthKey;
        private readonly BlackboardKey<float> maxHealthKey;
        private readonly ResponseCurve curve;

        public HealthConsideration(
            string name,
            BlackboardKey<float> currentHealthKey,
            BlackboardKey<float> maxHealthKey,
            ResponseCurve curve = null)
        {
            Name = name;
            this.currentHealthKey = currentHealthKey;
            this.maxHealthKey = maxHealthKey;
            this.curve = curve ?? ResponseCurve.Linear();
        }

        /// <summary>
        /// 使用固定最大生命值
        /// </summary>
        public HealthConsideration(
            string name,
            BlackboardKey<float> currentHealthKey,
            float maxHealth,
            ResponseCurve curve = null)
        {
            Name = name;
            this.currentHealthKey = currentHealthKey;
            fixedMaxHealth = maxHealth;
            this.curve = curve ?? ResponseCurve.Linear();
        }

        private readonly float fixedMaxHealth = -1f;

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(currentHealthKey, out float currentHealth))
            {
                return 0f;
            }

            float maxHealth;
            if (fixedMaxHealth > 0)
            {
                maxHealth = fixedMaxHealth;
            }
            else if (!blackboard.TryGetValue(maxHealthKey, out maxHealth))
            {
                return 0f;
            }

            float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);
            return curve.Evaluate(healthPercent);
        }
    }

    /// <summary>
    /// 时间考量 - 基于某个事件发生后的时间
    /// </summary>
    public class TimeSinceConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<float> timestampKey;
        private readonly ResponseCurve curve;

        public TimeSinceConsideration(
            string name,
            BlackboardKey<float> timestampKey,
            ResponseCurve curve = null)
        {
            Name = name;
            this.timestampKey = timestampKey;
            // 默认：时间越长，分数越高（0-10秒）
            this.curve = curve ?? ResponseCurve.Linear(1f, 0f, 10f);
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(timestampKey, out float timestamp))
            {
                return 1f; // 如果没有记录，假设时间很长
            }

            float elapsed = Time.time - timestamp;
            return curve.Evaluate(elapsed);
        }
    }
}
