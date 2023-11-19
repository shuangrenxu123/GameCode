using Fight;

namespace HTN
{
    /// <summary>
    /// 该类是原子行为中的行为，因为我们有可能在某些情况下会出现相同的行为，但是他的Condition与Effect不同的情况
    /// </summary>
    public abstract class BevBase
    {
        public abstract HTNResults Execute();
    }
}
