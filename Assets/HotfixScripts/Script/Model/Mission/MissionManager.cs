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
        public void ReceiveMission(int id)
        {
            //TODO 该任务是否完成了

            if (missions.ContainsKey(id))
            {
                Debug.LogError($"任务{id}已经存在");
                return;
            }

            var mission = new Mission(id);
            missions.Add(id, mission);
            OnMissionReceive?.Invoke(mission);
        }

        /// <summary>
        /// 尝试完成一个任务
        /// </summary>
        public void CompleteMission(int id)
        {
            if (!missions.TryGetValue(id, out var mission))
            {
                Debug.LogError($"任务{mission}不存在");
                return;
            }

            if (!mission.isComplete)
            {
                Debug.LogError($"任务{mission}不满足完成条件");
                return;
            }

            OnMissionComplete?.Invoke(mission);
            missions.Remove(id);
            //ToDo:添加奖励
        }

        /// <summary>
        /// 触发一个事件
        /// </summary>
        /// <param name="message"></param>
        public void TriggerMission(MissionMessage message)
        {
            if (missions.Count == 0)
            {
                return;
            }
            foreach (var mission in missions)
            {
                mission.Value.ExecuteMessage(message);
            }
        }

    }
}
