using System;
using System.Collections.Generic;
using UnityEngine;
namespace PreCondition
{
    public enum PreConditionType
    {
        BagItemCount,
        QuestCompleted,
        QuestExist,
        PropertyCheck,
        LevelCheck,
    }
    public interface IPreConditionChecker
    {
        PreConditionType PreConditionType { get; }

        public bool CheckPreCondition();
    }

    [Serializable]
    public class PreCondition
    {
        public PreConditionType preConditionType;
        public List<int> prams;
    }
}
