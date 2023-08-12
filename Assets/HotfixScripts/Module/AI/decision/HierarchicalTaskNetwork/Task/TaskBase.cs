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
        /// Ç°ÖÃÌõ¼ş
        /// </summary>
        public Cond cond;

        public TaskBase(string name, TaskType type, Cond c)
        {
            Name = name;
            this.type = type;
            cond = c;
        }
    }
}
