using System.Collections.Generic;
using System.Linq;
namespace HTN
{
    /// <summary>
    /// �滮��,�����������ʵ�����
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
        /// ���������������������б�
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// ������������״̬������Ϣ
        /// </summary>
        public abstract void BuildWorldState();
    }
}
