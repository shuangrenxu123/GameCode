using Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffManager
{
    public CombatEntity entity;
    private List<BuffBase> _buffs;
    /// <summary>
    /// 获得了某个buff的事件,我们或许有个护盾buff，当受到伤害的时候可以获得另一个回血buff
    /// </summary>
    public Action<BuffBase> OnAddBuff;
    /// <summary>
    /// 移除了某个buff的事件
    /// </summary>
    public Action<BuffBase> OnRemoveBuff;
    /// <summary>
    /// 当前所拥有的buff的Tag
    /// </summary>
    private HashSet<BuffTag> _tags;

    private HashSet<BuffTag> _mutexTags;
    public BuffManager(CombatEntity me)
    {
        _buffs = new();
        _tags = new HashSet<BuffTag>();
        _mutexTags = new HashSet<BuffTag>();
        this.entity = me;
    }
    public void OnUpdate()
    {
        for (int i = _buffs.Count - 1; i >= 0; i--)
        {
            var buff = _buffs[i];
            buff.nowtime += Time.deltaTime;
            if (buff.nowtime >= buff.data.Maxtime)
            {
                RemoveBuff(buff);
                //entity.stateUI.RemoveBuff(buff);
                OnRemoveBuff?.Invoke(buff);
                return;
            }
            buff.OnTrigger();
        }
    }

    public BuffBase AddBuff(string buffName, CombatEntity creater)
    {
        var buff = BuffFactory.CreateBuff(buffName, creater, this);
        bool canAdd = buff.data.Tag.ToList().Intersect(_mutexTags.ToList()).Count() == 0;
        if (canAdd)
        {
            foreach (var i in _buffs)
            {
                if (buffName == i.data.name)
                {
                    i.OnRefresh();
                    return i;
                }
            }
            buff.OnAdd();
            _buffs.Add(buff);
            foreach (var i in buff.data.Tag)
            {
                _tags.Add(i);
            }
            OnAddBuff?.Invoke(buff);
            return buff;
        }
        return null;
    }
    public void RemoveBuff(BuffBase buff)
    {
        buff.OnDestory();
        _buffs.Remove(buff);
        foreach (var i in buff.data.Tag)
        {
            _tags.Remove(i);
        }
    }
}
