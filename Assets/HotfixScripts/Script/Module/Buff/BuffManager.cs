using System;
using System.Collections.Generic;
using System.Linq;
using Fight;
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
            if (buff == null)
            {
                continue;
            }

            buff.nowTime += Time.deltaTime;
            if (buff.data != null && buff.data.Maxtime > 0 && buff.nowTime >= buff.data.Maxtime)
            {
                RemoveBuffInternal(buff, invokeEvent: true);
                continue;
            }

            buff.OnTrigger();
        }
    }
    public void AddBuff(string buffName, CombatEntity creator)
    {
        var buff = BuffFactory.CreateBuff(buffName, creator, this);
        if (buff == null)
        {
            Debug.Log($"{buffName} : 不存在");
            return;
        }
        bool canAdd = buff.data.Tag.ToList().Intersect(_mutexTags.ToList()).Count() == 0;
        if (canAdd)
        {
            foreach (var i in _buffs)
            {
                if (buffName == i.data.name)
                {
                    i.OnRefresh();
                    return;
                }
            }
            buff.OnAdd();
            _buffs.Add(buff);
            if (buff.data != null && buff.data.Tag != null)
            {
                foreach (var i in buff.data.Tag)
                {
                    _tags.Add(i);
                }
            }
            OnAddBuff?.Invoke(buff);
            return;
        }
        return;
    }
    public void AddBuff(string buffName)
    {
        AddBuff(buffName, entity);
    }
    public void RemoveBuff(BuffBase buff)
    {
        RemoveBuffInternal(buff, invokeEvent: true);
    }

    private bool RemoveBuffInternal(BuffBase buff, bool invokeEvent)
    {
        if (buff == null || !_buffs.Contains(buff))
        {
            return false;
        }

        buff.OnRemove();
        buff.OnDestroy();
        _buffs.Remove(buff);

        if (buff.data != null && buff.data.Tag != null)
        {
            foreach (var i in buff.data.Tag)
            {
                _tags.Remove(i);
            }
        }

        if (invokeEvent)
        {
            OnRemoveBuff?.Invoke(buff);
        }

        return true;
    }
}
