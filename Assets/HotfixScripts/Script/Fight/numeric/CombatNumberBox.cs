namespace Fight
{
    public class CombatNumberBox
    {
        public IntCollector Atk;
        public IntCollector Def;
        //public IntCollector Speed;
        
        public void Init()
        {
            Atk = new (PropertySourceType.Self);
            Def = new (PropertySourceType.Self);
            //Speed = new ();
        }
    }
}
