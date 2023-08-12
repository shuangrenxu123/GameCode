public interface IBuff
{
    /// <summary>
    /// 获得buff以后
    /// </summary>
    public void OnAdd();
    /// <summary>
    /// 触发buff
    /// </summary>
    public void OnTrigger();
    /// <summary>
    /// 再次获得buff以后
    /// </summary>
    public void OnRefresh();
    /// <summary>
    /// 因为其他因素被移除
    /// </summary> 
    public void OnRemove();
    /// <summary>
    /// 时间结束后由Buffmanager移除
    /// </summary>
    public void OnDestory();

}
