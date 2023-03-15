using Fight;
/// <summary>
/// buff的状态，
/// </summary>
public enum BuffState
{
    waiting,
    runing,
    end
}
public abstract class BuffBase
{
    public BuffState buffState;

    /// <summary>
    /// 已经持续的时间
    /// </summary>
    public float nowtime = 0;
    /// <summary>
    /// buff持续时间
    /// </summary>
    public float Maxtime;
    /// <summary>
    /// 当前叠加的层数
    /// </summary>
    float currNumber;
    /// <summary>
    /// 释放者
    /// </summary>
    public CombatEntity Creator;
    /// <summary>
    /// 目标
    /// </summary>
    public CombatEntity taget;

    public BuffDataBase data;
    public BuffBase(BuffState state, CombatEntity c, CombatEntity t)
    {
        buffState = state;
        Creator = c;
        taget = t;
    }
    /// <summary>
    /// 获得buff
    /// </summary>
    public abstract void Add();
    /// <summary>
    /// 每秒的buff更新
    /// </summary>
    public abstract void OnUpdate();

    /// <summary>
    /// 移除buff
    /// </summary>
    public abstract void OnRemove();
    /// <summary>
    /// 再次获得相同buff的刷新
    /// </summary>
    public abstract void OnRefresh();

}
