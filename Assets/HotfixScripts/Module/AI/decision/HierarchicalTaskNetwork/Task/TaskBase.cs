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
        /// 前置条件������
        /// </summary>
        public List<HTNCondition> conds;
        public DomainBase domain;
        public WorldState WorldState
        {
            get
            {
                // 确保domain和domain.ws都不为null
                return (domain != null && domain.ws != null) ? domain.ws : null;
            }
        }
        public TaskBase(DomainBase domain, string name, TaskType type, List<HTNCondition> c)
        {
            this.domain = domain;
            Name = name;
            this.type = type;
            conds = c ?? new List<HTNCondition>();
        }
        public TaskBase(DomainBase domain, string name, TaskType type)
        {
            this.domain = domain;
            Name = name; this.type = type;
            conds = new List<HTNCondition>();
        }
        public void AddCondition(HTNCondition c)
        {
            if (conds == null)
            {
                conds = new List<HTNCondition>();
            }
            if (c != null)
            {
                conds.Add(c);
            }
        }
        public void RemoveCondition(HTNCondition c)
        {
            if (conds != null && c != null)
            {
                conds.Remove(c);
            }
        }
        public virtual bool CheckTaskConditions(WorldState worldState = null)
        {
            // 使用传入的世界状态，如果未传入则使用默认的世界状态
            WorldState checkState = worldState ?? WorldState;
            
            // 检查基本条件
            if (conds == null || conds.Count == 0 || checkState == null)
            {
                return true; // 如果没有条件或者世界状态为null，认为条件成立
            }

            foreach (HTNCondition c in conds)
            {
                if (c == null || !c.Check(checkState))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
