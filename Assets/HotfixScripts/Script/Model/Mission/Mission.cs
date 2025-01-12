using System.Collections.Generic;
using UnityEngine;

namespace Mission
{

    public class Mission
    {
        MissionPrototype prototype;
        List<MissionRequireState> requireStates = new();
    }
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
    public abstract class MissionRequire
    {

    }
    public abstract class MissionRequireState
    {
        private MissionRequire require;
        protected MissionRequireState(MissionRequire require)
        {
            this.require = require;
        }

        public abstract void ExecuteAction(MissionMessage message);
    }

    public struct MissionMessage
    {
        public MissionEventType type;
        public int key;
        public int value;
    }

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