using System.Collections.Generic;

namespace HTN
{
    /// <summary>
    /// 通过方法继续分解的复合任务。
    /// </summary>
    public class CompoundTask : Task
    {
        public List<Method> Methods { get; } = new();

        public Method FindSatisfiedMethod(
            AIBlackboard.Blackboard source,
            AIBlackboard.Blackboard changes,
            int startIndex = 0)
        {
            throw new System.NotImplementedException();
        }
    }
}
