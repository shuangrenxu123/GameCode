using System.Collections.Generic;
namespace GOAP
{
    public interface IGoap
    {
        public HashSet<KeyValuePair<string, object>> GetWorldState();

        public HashSet<Goal> CreateGoalState();

        public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions);

        /// <summary>
        /// 执行完毕
        /// </summary>
        public void ActionFinished();
        /// <summary>
        /// 其中一个action执行失败了
        /// </summary>
        /// <param name="aborte"></param>
        public void planAborted(GoapAction aborte);
        /// <summary>
        /// 没有找到目标对应的plan
        /// </summary>
        /// <param name="failedGoal"></param>
        public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal);
        /// <summary>
        /// 如果可以执行nextAction则返回true，否则为false
        /// </summary>
        /// <param name="nextAction"></param>
        /// <returns></returns>
        //bool moveAgent(GoapAction nextAction);
        HashSet<GoapAction> InitAction();
    }
    public class Goal
    {
        public Goal(HashSet<KeyValuePair<string, object>> goal, int p)
        {
            this.goal = goal;
            Priority = p;
        }
        /// <summary>
        /// 目标的期望世界状态。
        /// </summary>
        public HashSet<KeyValuePair<string, object>> goal;
        /// <summary>
        /// 目标的优先级
        /// </summary>
        public int Priority;
    }

}