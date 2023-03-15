using Fight;
using UnityEngine;

public class zhongdu : BuffBase
{
    public zhongdu(BuffState state, CombatEntity c, CombatEntity t) : base(state, c, t)
    {
        base.Maxtime = 5f;
        data = new BuffDataBase(2);
    }

    /// <summary>
    /// 添加后
    /// </summary>
    public override void Add()
    {
        Debug.Log("获得中毒buff");
    }

    /// <summary>
    /// 再次获得后的刷新
    /// </summary>
    public override void OnRefresh()
    {

    }

    public override void OnRemove()
    {
        Debug.Log("中毒结束");
    }

    public override void OnUpdate()
    {
        if (nowtime < Maxtime)
        {
            Debug.Log("我中毒了");
            nowtime += Time.deltaTime;
        }
        else
        {
            buffState = BuffState.end;
        }
    }
}
