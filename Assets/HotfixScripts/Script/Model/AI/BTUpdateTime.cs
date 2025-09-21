using BT;
using System.Collections.Generic;
using UnityEngine;

public class BTUpdateTime<TKey, TValue> : BTAction<TKey, TValue>
{
    public List<TKey> timers;
    public BTUpdateTime()
    {
        timers = new List<TKey>();
    }
    public BTNode<TKey, TValue> AddTimer(TKey name)
    {
        timers.Add(name);
        return this;
    }
    protected override BTResult Execute()
    {
        foreach (var name in timers)
        {
            if (database.CheckDataNull(name))
            {
                database.SetData(name, (dynamic)0f);
            }
            var value = database.GetData<float>(name);
            value += Time.deltaTime;
            database.SetData(name, (dynamic)value);
        }
        return BTResult.Success;
    }
}
