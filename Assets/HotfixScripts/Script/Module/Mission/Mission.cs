using System.Collections.Generic;
using UnityEngine;

namespace Mission
{
    /// <summary>
    /// 一个任务
    /// </summary>
    public class Mission
    {
        MissionPrototype prototype;
        Dictionary<MissionRequireState, bool> requireStates = new();
        public bool isComplete { get; private set; }

        public Mission(int missionId)
        {

        }
        public bool ExecuteMessage(MissionMessage message)
        {
            foreach (var requireState in requireStates)
            {
                var result = requireState.Key.ExecuteMessage(message);
                requireStates[requireState.Key] = result;
            }

            isComplete = CheckComplete();
            return isComplete;
        }

        private bool CheckComplete()
        {
            foreach (var requireState in requireStates)
            {
                if (requireState.Value == false)
                {
                    return false;
                }
            }
            return true;
        }

    }
    /// <summary>
    /// 任务的原型，即配置文件
    /// </summary>
    public class MissionPrototype
    {
        public int id;
        public MissionRequireMode requireMode;
        /// <summary>
        /// 需求
        /// </summary>
        public List<MissionRequire> require;
        public List<MissionReward> rewards;

    }
    public class MissionRequire
    {
        public MissionEventType type;
        public int key;
        public int value;
    }
    /// <summary>
    /// 任务的需求完成状态
    /// </summary>
    public abstract class MissionRequireState
    {
        /// <summary>
        /// 原始需求
        /// </summary>
        private MissionRequire require;

        /// <summary>
        /// 是否完成
        /// </summary>
        bool isComplete = false;

        protected MissionRequireState(MissionRequire require)
        {
            this.require = require;
        }

        public abstract bool ExecuteMessage(MissionMessage message);
    }
    /// <summary>
    /// 任务的触发信息，比如获得了物品或者杀死了敌人
    /// </summary>
    public struct MissionMessage
    {
        public MissionEventType type;
        public int key;
        public int value;
    }

    /// <summary>
    /// 任务奖励
    /// </summary>
    public class MissionReward
    {
        MissionRewardType type;
    }

    public enum MissionRequireMode
    {
        /// <summary>
        ///  所有的得得完成
        /// </summary>
        All,

        /// <summary>
        /// 任意其中一个
        /// </summary>
        Any

    }

    public enum MissionEventType
    {
        KillEnemy,
        Dialogue,
        UseProp,
        Trigger
    }

    public enum MissionRewardType
    {
        Item,
        Exp,
    }
}