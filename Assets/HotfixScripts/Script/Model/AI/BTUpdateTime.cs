using System.Collections.Generic;
using BT;
using UnityEngine;

public class BTUpdateTime : BTAction
{
    public List<string> timers;
    public BTUpdateTime()
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
            if (database.ContainsData<float>(name))
            {
                database.SetValue(name, 0f);
            }
            var value = database.GetValue<float>(name);
            value += Time.deltaTime;
            database.SetValue(name, value);
        }
        return BTResult.Success;
    }
}
