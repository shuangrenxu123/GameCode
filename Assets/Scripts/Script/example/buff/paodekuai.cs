using Fight;
using UnityEngine;

public class paodekuai : BuffBase
{
    public paodekuai(BuffState state, CombatEntity c, CombatEntity t, float time) : base(state, c, t)
    {
        data = new BuffDataBase(1);
        base.Maxtime = 10;
    }

    public override void Add()
    {
        Debug.Log("收到了buff");
    }

    public override void OnRefresh()
    {

    }

    public override void OnRemove()
    {
        Debug.Log("跑得快结束了");
    }

    public override void OnUpdate()
    {
        if (nowtime >= Maxtime && buffState == BuffState.runing)
        {
            buffState = BuffState.end;
        }
        else
        {
            nowtime += Time.deltaTime;
        }
    }
}
