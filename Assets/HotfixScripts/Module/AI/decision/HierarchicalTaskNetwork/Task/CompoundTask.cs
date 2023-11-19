using System.Collections.Generic;
namespace HTN
{
    public class Method
    {
        public int id;
        // 这里写上一个条件可以快速跳过检测
        public HTNCondition cond;
        /// <summary>
        /// 该Method下的子方法,因为有可能会有分支，所以应该是写Taskbase
        /// </summary>
        public List<TaskBase> SubTasks;
        public Method(int id, HTNCondition c = null)
        {
            cond = c;
            this.id = id;
            SubTasks = new List<TaskBase>();
        }
        public Method AddTask(TaskBase task)
        {
            if (task != null)
                SubTasks.Add(task);
            return this;
        }
    }

    public class CompoundTask : TaskBase
    {
        public List<Method> methods;
        public CompoundTask(DomainBase domain, string name, TaskType type, List<HTNCondition> c = null) : base(domain, name, type, c)
        {
            methods = new List<Method>();
        }
        public void Reset()
        {
            methods.Clear();
            conds = null;
        }

        public CompoundTask AddMethod(Method v)
        {
            if (v != null)
            {
                methods.Add(v);
            }
            return this;
        }

        /// <summary>
        /// 传入世界状态，然后检查哪个method是复合结果的然后返回对应的method
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public Method FindValidMethod(WorldState ws)
        {
            foreach (var i in methods)
            {
                if (i.cond.Check(ws))
                {
                    return i;
                }
            }
            return null;
        }
    }
}
