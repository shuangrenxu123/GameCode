using Fight;

public class PlayerCombatEntity : CombatEntity
{
    public override void Init(int h)
    {
        //stateUI = UIManager.Instance.GetUIWindow<StateUI>();
        hp.Init(true);
        hp.SetMaxValue(h);
        numberBox.Init();
        numberBox.Speed.SetBase(5);
        numberBox.Atk.SetBase(10);
    }
    public void AddBuff(string name)
    {
        var buff = buffManager.AddBuff(name, this);
        //stateUI.AddBuff(buff);
    }
}
