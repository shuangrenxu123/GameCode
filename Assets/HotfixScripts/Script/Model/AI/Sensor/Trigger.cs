using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Character.AI.Sensor
{
    public interface ISensorTrigger
    {
        protected abstract SensorType sensorType { get; }

        public abstract void Notify();
    }
}
