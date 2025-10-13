using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.AI.Sensor
{
    public class SoundTrigger : MonoBehaviour, ISensorTrigger
    {
        SensorType ISensorTrigger.sensorType => SensorType.Sound;

        [SerializeField, LabelText("通知间隔")]
        float notifyInterval = 0.5f;

        [SerializeField, LabelText("通知半径")]
        float notifyRadius = 10f;

        [SerializeField, LabelText("需要探测的目标所在的层")]
        LayerMask TargetMask = 1;
        Collider[] colliders;

        float timer = 0f;

        void Awake()
        {
            colliders = new Collider[10];
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > notifyInterval)
            {
                Notify();
                timer = 0f;
            }
        }

        public void Notify()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, notifyRadius, colliders, TargetMask);

            for (int i = 0; i < count; i++)
            {
                // var targetCollider = triggers[i];

                // Transform target = targetCollider.transform;
                // Vector3 dirToTarget = (target.position - transform.position).normalized;
                // float distToTarget = Vector3.Distance(transform.position, target.position);

                // if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ObstacleMask))
                // {
                //     // 4.成功看见目标！内部生成一个刺激源并上报
                //     sensorManager.OnStimulusSensed(new SensorData(
                //         sensorType,
                //         target.position,
                //         this
                //     ));
                // }

            }
        }

    }
}