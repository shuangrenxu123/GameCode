namespace HTN
{
    /// <summary>
    /// ����Task��ǰ������
    /// </summary>
    public abstract class HTNCondition
    {
        public abstract bool Check(WorldState ws);
    }
}
