using System.Collections.Generic;
using UnityEngine;

public class TaskMgr : MonoBehaviour
{
    public List<TaskBase> tasks;
    public List<TaskBase> finishTasks;

    public TaskMgr()
    {
        tasks = new List<TaskBase>();
        finishTasks = new List<TaskBase>();
    }
    public void AddTask(TaskBase task)
    {
        tasks.Add(task);
    }
    public void FinishTask(int id)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].GetTaskId() == id)
            {
                finishTasks.Add(tasks[i]);
                tasks.RemoveAt(i);
                return;
            }
        }
    }

    public void UpdateTask(int id, int num)
    {
        foreach (var i in tasks)
        {
            i.UpdateTask(id, num);
        }
    }
}
