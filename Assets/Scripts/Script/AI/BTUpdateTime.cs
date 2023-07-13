using BT;
using System.Collections.Generic;
using UnityEngine;

public class BTUpdateTime : BTAction
{
    public List<string> timers;
    public BTUpdateTime(string name) : base(name)
    {
        timers = new List<string>();
    }
    public BTNode AddTimer(string name)
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
                database.SetData(name, 0f);
            }
            var value = database.GetData<float>(name);
            value += Time.deltaTime;
            database.SetData(name, value);
        }
        return BTResult.Success;
    }
}
