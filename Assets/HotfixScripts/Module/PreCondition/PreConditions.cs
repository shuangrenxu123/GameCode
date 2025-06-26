using UnityEngine;

namespace PreCondition
{
    public class BagPreCondition : IPreConditionChecker
    {
        public PreConditionType PreConditionType => PreConditionType.BagItemCount;

        public bool CheckPreCondition()
        {
            return true;
        }
    }
    public class QuestCompletedPreCondition : IPreConditionChecker
    {
        public PreConditionType PreConditionType => PreConditionType.QuestCompleted;

        public bool CheckPreCondition()
        {
            return true;
        }
    }
}
