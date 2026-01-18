using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Character.AI.Sensor
{
    public interface ISensorTrigger
    {
        public abstract void Notify();
    }
}
