using System.Collections;
using System.Collections.Generic;
using AIBlackboard;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.AI.Sensor
{
    public enum SensorType
    {
        Sight,
        Sound
    }

    public struct SensorData
    {

        /// <summary>
        /// 触发器坐标
        /// </summary>
        public Vector3 triggerPosition;

        /// <summary>
        /// 目标接受信息的感知器
        /// </summary>
        public Sensor targetSensor;
        public SensorData(Vector3 triggerPosition, Sensor targetSensor)
        {
            this.triggerPosition = triggerPosition;
            this.targetSensor = targetSensor;
        }
    }

    public abstract class Sensor : MonoBehaviour
    {
        [SerializeField, LabelText("检测间隔时间"), ShowIf("activeExecution")]
        float checkInterval = 0.5f;

        protected Blackboard database;

        public abstract SensorType sensorType { get; }

        /// <summary>
        /// 是否主动观测
        /// </summary>
        protected abstract bool activeExecution { get; }
        float timer = 0;

        protected SensorManager sensorManager;

        public void Init(SensorManager sensorManager, Blackboard database)
        {
            this.sensorManager = sensorManager;
            this.database = database;
        }


        public void UpdateSensor()
        {
            if (!activeExecution)
                return;
            timer += Time.deltaTime;
            if (timer > checkInterval)
            {
                Detect();
                timer = 0;
            }
        }

        /// <summary>
        /// 当真正的检测到了信息
        /// </summary>
        /// <param name="trigger"></param>
        public abstract void Notify(SensorData trigger);

        /// <summary>
        /// 每帧检测
        /// </summary>
        protected virtual void Detect() { }

    }
}