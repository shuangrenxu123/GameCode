using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关系转化图
/// </summary>
public class FSMGraph
{
    private readonly Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>();
    private readonly string globaNode;
    /// <summary>
    /// 添加转化关系
    /// </summary>
    /// <param name="nodeName">节点名称</param>
    /// <param name="transitionNode">可以转化到的节点，比如走动和转化到战斗和逃跑</param>
    public void AddTransition(string nodeName, List<string> transitionNode)
    {
        if (transitionNode == null)
        {
            throw new System.Exception("他没有可以转化到的下一个节点");
        }
        if (graph.ContainsKey(nodeName))
        {
            Debug.LogWarning("节点已经存在了");
            return;
        }
        graph.Add(nodeName, transitionNode);
    }
    /// <summary>
    /// 检测是否可以转化
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public bool CanTransition(string from, string to)
    {
        if (!graph.ContainsKey(from))
        {
            return false;
        }
        if (to == globaNode)
            return true;
        return graph[from].Contains(to);
    }
}
