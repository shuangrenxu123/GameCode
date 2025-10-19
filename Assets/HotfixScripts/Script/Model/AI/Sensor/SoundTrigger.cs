using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.AI.Sensor
{
    public class SoundTrigger : MonoBehaviour, ISensorTrigger
    {

        [SerializeField, LabelText("通知间隔")]
        float notifyInterval = 0.5f;

        [SerializeField, LabelText("通知半径")]
        float notifyRadius = 10f;

        [SerializeField, LabelText("需要探测的目标所在的层")]
        LayerMask TargetMask = 1;

        [SerializeField, LabelText("是否可以穿透障碍物")]
        bool canPenetrateObstacle = false;

        [SerializeField, LabelText("障碍物所在的层"), ShowIf("canPenetrateObstacle")]
        LayerMask ObstacleMask = 1;

        Collider[] colliders;
        Dictionary<GameObject, bool> notifiedTargets;

        float timer = 0f;

        void Awake()
        {
            colliders = new Collider[10];
            notifiedTargets = new Dictionary<GameObject, bool>(10);
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
            notifiedTargets.Clear();

            for (int i = 0; i < count; i++)
            {
                var targetCollider = colliders[i];

                Transform target = targetCollider.transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                float distToTarget = Vector3.Distance(transform.position, target.position);

                var sensor = targetCollider.GetComponentInChildren<SoundSensor>();

                var sensorManager = targetCollider.GetComponentInChildren<SensorManager>();

                if (notifiedTargets.ContainsKey(target.gameObject))
                {
                    continue;
                }

                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, ObstacleMask)
                    || canPenetrateObstacle)
                {
                    // 4.成功看见目标！内部生成一个刺激源并上报
                    sensorManager.OnStimulusSensed(new SensorData(
                        transform.position,
                        sensor
                    ));
                    notifiedTargets.Add(target.gameObject, true);
                }
            }
        }
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, notifyRadius);
        }

    }
}