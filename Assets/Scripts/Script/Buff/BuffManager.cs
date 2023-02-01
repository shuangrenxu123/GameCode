using System.Collections.Generic;

public class BuffManager
{
    private List<BuffBase> buffs;

    public BuffManager()
    {
        buffs = new List<BuffBase>();
    }
    public void OnUpdate()
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            BuffBase buff = buffs[i];
            if (buff.buffState == BuffState.waiting)
            {

            }
            if (buff.buffState == BuffState.runing)
            {
                buff.OnUpdate();
            }
            else
            {
                buff.OnRemove();
                buffs.Remove(buff);
            }
        }
    }

    public void AddBuff(BuffBase buff)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].data.id == buff.data.id)
            {
                buffs[i].OnRefresh();
                return;
            }
        }
        buffs.Add(buff);
        buff.Add();
    }
}
