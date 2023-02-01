/// <summary>
/// buff��״̬��
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
    /// �Ѿ�������ʱ��
    /// </summary>
    public float nowtime = 0;
    /// <summary>
    /// buff����ʱ��
    /// </summary>
    public float Maxtime;
    /// <summary>
    /// ��ǰ���ӵĲ���
    /// </summary>
    float currNumber;
    /// <summary>
    /// �ͷ���
    /// </summary>
    public CombatEntity Creator;
    /// <summary>
    /// Ŀ��
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
    /// ���buff
    /// </summary>
    public abstract void Add();
    /// <summary>
    /// ÿ���buff����
    /// </summary>
    public abstract void OnUpdate();

    /// <summary>
    /// �Ƴ�buff
    /// </summary>
    public abstract void OnRemove();
    /// <summary>
    /// �ٴλ����ͬbuff��ˢ��
    /// </summary>
    public abstract void OnRefresh();

}
