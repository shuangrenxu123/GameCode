using AIBlackboard;
using Character.AI.Sensor;

namespace UtilityAI.Integration
{
    /// <summary>
    /// 传感器考量 - 基于传感器数据计算效用
    /// </summary>
    public class SensorConsideration : IConsideration
    {
        public string Name { get; }

        private readonly SensorType sensorType;
        private readonly BlackboardKey<SensorData> sensorDataKey;
        private readonly ResponseCurve curve;

        public SensorConsideration(
            string name,
            SensorType sensorType,
            BlackboardKey<SensorData> sensorDataKey,
            ResponseCurve curve = null)
        {
            Name = name;
            this.sensorType = sensorType;
            this.sensorDataKey = sensorDataKey;
            this.curve = curve ?? ResponseCurve.Linear();
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(sensorDataKey, out SensorData data))
            {
                return 0f;
            }

            // 可以根据传感器类型和数据计算效用
            return curve.Evaluate(1f); // 检测到则返回高分
        }
    }

    /// <summary>
    /// 威胁检测考量 - 检测是否有威胁
    /// </summary>
    public class ThreatDetectedConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<bool> threatDetectedKey;
        private readonly BlackboardKey<float> threatLevelKey;
        private readonly ResponseCurve curve;

        public ThreatDetectedConsideration(
            string name,
            BlackboardKey<bool> threatDetectedKey,
            BlackboardKey<float> threatLevelKey = default,
            ResponseCurve curve = null)
        {
            Name = name;
            this.threatDetectedKey = threatDetectedKey;
            this.threatLevelKey = threatLevelKey;
            this.curve = curve ?? ResponseCurve.Linear();
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(threatDetectedKey, out bool detected) || !detected)
            {
                return 0f;
            }

            // 如果有威胁等级，使用它
            if (threatLevelKey.Id != 0 && blackboard.TryGetValue(threatLevelKey, out float level))
            {
                return curve.Evaluate(level);
            }

            return 1f;
        }
    }

    /// <summary>
    /// 视线考量 - 检查是否能看到目标
    /// </summary>
    public class LineOfSightConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<bool> hasLineOfSightKey;

        public LineOfSightConsideration(string name, BlackboardKey<bool> hasLineOfSightKey)
        {
            Name = name;
            this.hasLineOfSightKey = hasLineOfSightKey;
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValue(hasLineOfSightKey, out bool hasLOS))
            {
                return hasLOS ? 1f : 0f;
            }
            return 0f;
        }
    }

    /// <summary>
    /// 声音警觉考量 - 基于听到的声音
    /// </summary>
    public class SoundAlertConsideration : IConsideration
    {
        public string Name { get; }

        private readonly BlackboardKey<float> lastSoundTimeKey;
        private readonly BlackboardKey<float> soundIntensityKey;
        private readonly float alertDuration;
        private readonly ResponseCurve timeCurve;
        private readonly ResponseCurve intensityCurve;

        public SoundAlertConsideration(
            string name,
            BlackboardKey<float> lastSoundTimeKey,
            BlackboardKey<float> soundIntensityKey,
            float alertDuration = 5f,
            ResponseCurve timeCurve = null,
            ResponseCurve intensityCurve = null)
        {
            Name = name;
            this.lastSoundTimeKey = lastSoundTimeKey;
            this.soundIntensityKey = soundIntensityKey;
            this.alertDuration = alertDuration;
            this.timeCurve = timeCurve ?? ResponseCurve.InverseLinear(0f, alertDuration);
            this.intensityCurve = intensityCurve ?? ResponseCurve.Linear();
        }

        public float Evaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValue(lastSoundTimeKey, out float lastSoundTime))
            {
                return 0f;
            }

            float elapsed = UnityEngine.Time.time - lastSoundTime;
            if (elapsed > alertDuration)
            {
                return 0f;
            }

            float timeScore = timeCurve.Evaluate(elapsed);

            if (blackboard.TryGetValue(soundIntensityKey, out float intensity))
            {
                float intensityScore = intensityCurve.Evaluate(intensity);
                return timeScore * intensityScore;
            }

            return timeScore;
        }
    }
}
