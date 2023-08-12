using Fight;

public class PlayerCombatEntity : CombatEntity
{
    public override void Init(int h)
    {
        stateUI = WindowsManager.Instance.GetUiWindow<StateUI>();
        hp.Init(true);
        hp.SetMaxValue(h);
        numberBox.Init();
        numberBox.Speed.SetBase(5);
        numberBox.Atk.SetBase(10);

        //AddBuff("poisoned");
        //AddBuff("hemophagia");
    }
    public void AddBuff(string name)
    {
        var buff = buffManager.AddBuff(name, this);
        stateUI.AddBuff(buff);
    }
}
