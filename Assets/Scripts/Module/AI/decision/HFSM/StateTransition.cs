using System.Collections.Generic;

namespace HFSM
{


    /// <summary>
    /// 关系转化的连线
    /// </summary>
    public class StateTransition
    {
        public string startState;
        public string endState;
        public HashSet<StateCondition> conditions;
        public StateTransition(string from, string to)
        {
            startState = from;
            endState = to;
            conditions = new HashSet<StateCondition>();
        }
        public void AddCondition(StateCondition condition)
        {
            if (conditions.Contains(condition))
            {
                return;
            }
            conditions.Add(condition);
        }
        /// <summary>
        /// 检测是否可以切换至下一个状态
        /// </summary>
        /// <returns></returns>
        public bool OnCheck()
        {
            foreach (var c in conditions)
            {
                if (c.Check() == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}