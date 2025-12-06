using Character.AI.Sensor;
using UnityEngine;

namespace Enemy.AI.Utility
{
    /// <summary>
    /// 简单的敌人感知器，将玩家距离和可见状态写入黑板
    /// </summary>
    public class EnemyPerceptionSensor : Sensor
    {
        [SerializeField, Range(1f, 50f)]
        float viewDistance = 15f;

        [SerializeField]
        Transform trackedTarget;

        public override SensorType sensorType => SensorType.Sight;
        protected override bool activeExecution => true;

        public void AssignTarget(Transform target)
        {
            trackedTarget = target;
        }

        public void ConfigureRange(float distance)
        {
            viewDistance = Mathf.Max(0.1f, distance);
        }

        public void ForceSample()
        {
            SampleTarget();
        }

        protected override void Detect()
        {
            SampleTarget();
        }

        public override void Notify(SensorData trigger)
        {
            // 依赖主动检测，触发事件留空
        }

        void SampleTarget()
        {
            if (trackedTarget == null || database == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, trackedTarget.position);
            bool visible = distance <= viewDistance;

            database.SetValue(EnemyAIDatabaseKey.PlayerDistance.ToString(), distance);
            database.SetValue(EnemyAIDatabaseKey.PlayerVisible.ToString(), visible);
        }
    }
}
