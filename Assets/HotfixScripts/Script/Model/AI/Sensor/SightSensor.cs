using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
namespace Character.AI.Sensor
{
    public enum ShapeType
    {

        Rect,

        Sector,

        Circle
    }

    /// <summary>
    /// 视觉感知器
    /// </summary>
    public class SightSensor : Sensor
    {
        [SerializeField, LabelText("视野距离")]
        float viewDistance = 5;

        [SerializeField, Range(0, 360), LabelText("视野角度")]
        float viewAngle = 60;

        [SerializeField, LabelText("需要探测的目标所在的层")]
        LayerMask TargetMask = 1;

        [SerializeField, LabelText("会阻挡视线的障碍物所在的层")]
        LayerMask ObstacleMask = 1;

        Collider[] triggers;

        protected ShapeType shapeType;

        public override SensorType sensorType => SensorType.Sight;

        protected override bool activeExecution => true;

        private void Awake()
        {
            triggers = new Collider[20];
        }

        protected override void Detect()
        {
            // 1. 使用物理引擎在视野距离内查找所有可能的目标
            int count = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, triggers, TargetMask);

            for (int i = 0; i < count; i++)
            {
                var targetCollider = triggers[i];

                Transform target = targetCollider.transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                var tempAngle = Vector3.Angle(transform.forward, dirToTarget);
                // 2. 检查目标是否在视野角度内
                if (tempAngle < viewAngle / 2)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ObstacleMask))
                    {
                        // 4.成功看见目标！内部生成一个刺激源并上报
                        sensorManager.OnStimulusSensed(new SensorData(
                            sensorType,
                            target.position,
                            this
                        ));
                    }
                }
            }
        }

        public override void Notify(SensorData trigger)
        {
            Debug.Log("看到了敌人");
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            float halfAngle = viewAngle / 2;
            for (float angle = -halfAngle; angle <= halfAngle; angle += 6)
            {
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 direction = rotation * transform.forward;
                Gizmos.DrawRay(transform.position, direction * viewDistance);
            }
        }
    }
}