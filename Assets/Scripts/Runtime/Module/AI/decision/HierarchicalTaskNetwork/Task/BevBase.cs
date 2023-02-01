namespace HTN
{
    public abstract class BevBase
    {
        CombatEntity entity;
        public BevBase(CombatEntity e)
        {
            entity = e;
        }
        public abstract HTNResults Execute();
    }
}
