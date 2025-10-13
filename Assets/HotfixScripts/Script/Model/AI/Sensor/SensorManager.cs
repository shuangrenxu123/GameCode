using UnityEngine;

namespace Character.AI.Sensor
{
    public class SensorManager : MonoBehaviour
    {
        Sensor[] sensors;
        DataBase<string, object> dataBase;

        void Awake()
        {
            sensors = GetComponentsInChildren<Sensor>();
        }
        void Start()
        {
            if (dataBase == null)
            {
                dataBase = new();
            }

            foreach (var sensor in sensors)
            {
                sensor.Init(this, dataBase);
            }
        }
        void Update()
        {
            foreach (var sensor in sensors)
            {
                sensor.UpdateSensor();
            }
        }

        public void OnStimulusSensed(SensorData data)
        {
            foreach (var sensor in sensors)
            {
                if (sensor != data.targetSensor)
                    continue;

                sensor.Notify(data);
            }
        }

    }
}
