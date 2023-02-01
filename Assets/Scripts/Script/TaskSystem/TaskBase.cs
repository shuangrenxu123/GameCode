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
    /// ������������
    /// </summary>
    /// <param name="id"></param>
    /// <param name="num"></param>
    public abstract void UpdateTask(int id,int num);
    public abstract int GetTaskId();
    /// <summary>
    /// ��ý���
    /// </summary>
    /// <returns></returns>
    public abstract Dictionary<int, int> GetremunerationItems();
    /// <summary>
    /// �Ƿ����������������
    /// </summary>
    /// <returns></returns>
    protected abstract void isEligible();
    protected abstract void CanAcceptTask();
}
