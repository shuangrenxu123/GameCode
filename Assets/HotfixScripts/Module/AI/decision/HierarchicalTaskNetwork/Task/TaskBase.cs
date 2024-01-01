using System.Collections.Generic;

namespace HTN
{
    public enum TaskType
    {
        Primitive,
        Compound,
    }
    public abstract class TaskBase
    {
        public string Name;
        public TaskType type;
        /// <summary>
        /// Ç°ÖÃÌõ¼þ
        /// </summary>
        public List<HTNCondition> conds;
        public DomainBase domain;
        public WorldState WorldState => domain.ws;
        public TaskBase(DomainBase domain, string name, TaskType type, List<HTNCondition> c)
        {
            this.domain = domain;
            Name = name;
            this.type = type;
            conds = c;
        }
        public TaskBase(DomainBase domain, string name, TaskType type)
        {
            this.domain = domain;
            Name = name; this.type = type;
            conds = new List<HTNCondition>();
        }
        public void AddCondition(HTNCondition c)
        {
            conds.Add(c);
        }
        public void RemoveCondition(HTNCondition c)
        {
            conds.Remove(c);
        }
        public virtual bool CheckTaskConditions()
        {
            foreach (HTNCondition c in conds)
            {
                if (!c.Check(WorldState))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
