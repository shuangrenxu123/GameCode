using Fight;
using System.Collections.Generic;
using System.Linq;
namespace HTN
{
    /// <summary>
    /// 规划域,具体子类可以实现这个
    /// </summary>
    public abstract class DomainBase
    {
        public List<TaskBase> TaskList;
        public WorldState ws;
        public CombatEntity entity;
        public DomainBase(WorldState ws)
        {
            TaskList = new List<TaskBase>();
        }
        public void Init(CombatEntity entity, WorldState ws)
        {
            this.entity = entity;
            this.ws = ws;
        }
        public void AddTask(TaskBase task)
        {
            TaskList.Add(task);
        }
        public void RemoveTask(string name)
        {
            TaskList = TaskList.Where(x => x.Name != name).ToList();
        }
        /// <summary>
        /// 用于声明所包含的任务列表
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 用于声明世界状态基本信息
        /// </summary>
        public abstract void BuildWorldState();
    }
}
