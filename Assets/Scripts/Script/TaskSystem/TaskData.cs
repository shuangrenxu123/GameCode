using System.Collections.Generic;

public class TaskData
{
    /// <summary>
    /// 前置任务id
    /// </summary>
    public HashSet<int> predecessorTasksId;
    public int id;
    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskType type;
    public string name;
    /// <summary>
    /// 任务描述
    /// </summary>
    public string description;
    /// <summary>
    /// 是否完成
    /// </summary>
    public bool isFinish = false;

    public bool CanAppect = true;
    /// <summary>
    /// 奖励物品id
    /// </summary>
    public Dictionary<int, int> remunerationItemsId;
    /// <summary>
    /// 所需要的物品或需要击杀的怪物
    /// </summary>
    public Dictionary<int, int> filter;
}
public enum TaskType
{
    GetItem,
    Kill,
}

