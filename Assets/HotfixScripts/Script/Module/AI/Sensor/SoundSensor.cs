using UnityEngine;

namespace Character.AI.Sensor
{
    public class SoundSensor : Sensor
    {
        public override SensorType sensorType => SensorType.Sound;

        protected override bool activeExecution => false;

        public override void Notify(SensorData trigger)
        {
        }
    }
}
