using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTask : TaskBase
{
    public ItemTask(TaskData taskData,TaskMgr taskMgr) : base(taskData,taskMgr)
    {

    }

    public override Dictionary<int, int> GetremunerationItems()
    {
        if (Eligible)
        {
            return taskData.remunerationItemsId;
        }
        else
        {
            return null;
        }
    }
    public override int GetTaskId()
    {
        return taskData.id;
    }
    protected override void isEligible()
    {
        bool t = true;
        foreach (var i in taskData.filter)
        {
            t = t && (i.Value == 0);
        }
        Eligible = t;
    }
    public override void UpdateTask(int id, int num)
    {
        var n = taskData.filter[id];
        taskData.filter[id] = Math.Max(0,n - num);
        isEligible();
    }

    protected override void CanAcceptTask()
    {
        bool have;
        foreach (var i in taskData.predecessorTasksId)
        {
            have = false;
            for (int j = 0; j < TaskMgr.finishTasks.Count; j++)
            {
                if (TaskMgr.finishTasks[j].GetTaskId() == i)
                {
                    have=true;
                    break;
                }
            }
            if (have == false)
            {
                taskData.CanAppect = false;
                return;
            }
        }
    }
}
