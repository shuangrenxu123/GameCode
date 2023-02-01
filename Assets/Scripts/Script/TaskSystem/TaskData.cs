using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskData
{
    /// <summary>
    /// ǰ������id
    /// </summary>
    public HashSet<int> predecessorTasksId;
    public int id;
    /// <summary>
    /// ��������
    /// </summary>
    public TaskType type;
    public string name;
    /// <summary>
    /// ��������
    /// </summary>
    public string description;
    /// <summary>
    /// �Ƿ����
    /// </summary>
    public bool isFinish = false;

    public bool CanAppect = true;
    /// <summary>
    /// ������Ʒid
    /// </summary>
    public Dictionary<int, int> remunerationItemsId;
    /// <summary>
    /// ����Ҫ����Ʒ����Ҫ��ɱ�Ĺ���
    /// </summary>
    public Dictionary<int,int> filter;
}
public enum TaskType
{
    GetItem,
    Kill,
}

