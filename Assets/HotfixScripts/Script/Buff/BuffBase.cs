using Fight;
public abstract class BuffBase : IBuff
{
    public BuffManager BuffManager;
    /// <summary>
    /// 已经持续的时间
    /// </summary>
    public float nowtime = 0;
    public float maxTime { get { return data.Maxtime; } }
    /// <summary>
    /// 当前叠加的层数
    /// </summary>
    float currNumber;
    /// <summary>
    /// buff提供者
    /// </summary>
    public CombatEntity Creator;
    public BuffDataBase data;
    public BuffBase(BuffManager manager, CombatEntity c)
    {
        this.BuffManager = manager;
        Creator = c;
    }
    public virtual void OnAdd()
    {

    }

    public virtual void OnTrigger()
    {

    }

    public virtual void OnRefresh()
    {

    }

    public virtual void OnRemove()
    {

    }

    public virtual void OnDestory()
    {

    }
    /// <summary>
    /// 返回当前持续时间百分比
    /// </summary>
    /// <returns></returns>
    public float Percent()
    {
        return nowtime / maxTime;
    }
}
