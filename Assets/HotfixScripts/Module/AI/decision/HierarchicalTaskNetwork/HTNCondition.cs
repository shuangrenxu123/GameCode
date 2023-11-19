namespace HTN
{
    /// <summary>
    /// 进入Task的前置条件
    /// </summary>
    public abstract class HTNCondition
    {
        public abstract bool Check(WorldState ws);
    }
}
