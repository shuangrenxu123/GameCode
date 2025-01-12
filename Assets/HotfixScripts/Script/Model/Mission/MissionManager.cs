using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mission
{
    public class MissionManager : MonoSingleton<MissionManager>
    {
        public event Action<Mission> OnMissionReceive;

        public event Action<Mission> OnMissionComplete;

        private Dictionary<int, Mission> missions = new();

        /// <summary>
        /// 领取一个任务
        /// </summary>
        public void ReceiveMission()
        {

        }

        /// <summary>
        /// 完成一个任务
        /// </summary>
        private void CompleteMission()
        {

        }

        public void TriggerMission(MissionMessage message)
        {
            if (missions.Count == 0)
            {
                return;
            }
            foreach (var mission in missions)
            {

            }
        }

    }
}
