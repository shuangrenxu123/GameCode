using UnityEngine;

public class zhongdu : BuffBase
{
    public zhongdu(BuffState state, CombatEntity c, CombatEntity t) : base(state, c, t)
    {
        base.Maxtime = 5f;
        data = new BuffDataBase(2);
    }

    /// <summary>
    /// ��Ӻ�
    /// </summary>
    public override void Add()
    {
        Debug.Log("����ж�buff");
    }

    /// <summary>
    /// �ٴλ�ú��ˢ��
    /// </summary>
    public override void OnRefresh()
    {

    }

    public override void OnRemove()
    {
        Debug.Log("�ж�����");
    }

    public override void OnUpdate()
    {
        if (nowtime < Maxtime)
        {
            Debug.Log("���ж���");
            nowtime += Time.deltaTime;
        }
        else
        {
            buffState = BuffState.end;
        }
    }
}
