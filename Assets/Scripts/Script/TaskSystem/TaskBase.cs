using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskBase
{
    public TaskMgr TaskMgr;
    protected TaskData taskData;
    public bool Eligible = false;
    public TaskBase(TaskData taskData, TaskMgr taskMgr)
    {
        this.taskData = taskData;
        TaskMgr = taskMgr;
    }
    public virtual bool IsFinish()
    {
        return taskData.isFinish;
    }
    /// <summary>
    /// 更新任务需求
    /// </summary>
    /// <param name="id"></param>
    /// <param name="num"></param>
    public abstract void UpdateTask(int id,int num);
    public abstract int GetTaskId();
    /// <summary>
    /// 获得奖励
    /// </summary>
    /// <returns></returns>
    public abstract Dictionary<int, int> GetremunerationItems();
    /// <summary>
    /// 是否完成任务所有需求
    /// </summary>
    /// <returns></returns>
    protected abstract void isEligible();
    protected abstract void CanAcceptTask();
}
