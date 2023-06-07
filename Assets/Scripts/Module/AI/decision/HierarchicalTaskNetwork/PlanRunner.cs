using System.Collections.Generic;

namespace HTN
{
    public class PlanRunner
    {
        WorldState state;
        public PlanRunner(WorldState state)
        {
            this.state = state;
        }

        public void RunPlan(Queue<PrimitiveTask> tasks)
        {
            PrimitiveTask task = null;
            while (tasks.Count > 0)
            {
                task = tasks.Dequeue();
                var res = task.Execute();
                task.ApplyEffects(state);
                if (res == HTNResults.fail)
                {
                    return;
                }
            }
        }
    }
}
