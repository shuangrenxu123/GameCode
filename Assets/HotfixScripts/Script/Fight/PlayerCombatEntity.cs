using Fight;

public class PlayerCombatEntity : CombatEntity
{
    public override void Init(int h)
    {
        hp.Init(true);
        hp.SetMaxValue(h);
        numberBox.Init();
        //numberBox.Speed.SetBase(5);
        //numberBox.Atk.SetBase(10);
    }
    public void AddBuff(string name)
    {
        buffManager.AddBuff(name, this);
    }
}
