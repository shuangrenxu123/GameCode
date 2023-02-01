using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 有限状态机类
/// </summary>
public class FiniteStateMachine : MonoBehaviour
{
    protected IFsmNode currNode;
    protected IFsmNode preNode;
    public readonly List<IFsmNode> fsmNodes = new List<IFsmNode>();
    public FSMGraph graph;
    /// <summary>
    /// 运行有限状态机
    /// </summary>
    public void Start()
    {
        graph = new FSMGraph();
        Init();
    }
    public void Update()
    {
        if (currNode != null)
            currNode.Update(this.gameObject);
    }
    public virtual void Init()
    {
    }
    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="node"></param>
    public void ChangeNode(string node, object token)
    {
        IFsmNode fsmnode = GetNode(node);
        if (graph != null)
        {
            if (!graph.CanTransition(currNode.name, node))
            {
                Debug.LogError("状态切换不了");
                return;
            }
            preNode = currNode;
            currNode.Exit();
            currNode = fsmnode;
            currNode.Enter(token);
        }
        else
        {
            Debug.Log("状态表中不存在");
        }
    }
    public void AddNode(string nodeName, List<string> nodes = null)
    {
        IFsmNode node = GetNode(nodeName);
        if (!fsmNodes.Contains(node))
        {
            fsmNodes.Add(node);
            if (nodes != null)
            {
                graph.AddTransition(currNode.name, nodes);
            }
        }
        else
        {
            Debug.LogWarning("重复添加了状态");
        }
    }
    public void AddNode(IFsmNode node, List<string> nodes = null)
    {
        if (!fsmNodes.Contains(node))
        {
            fsmNodes.Add(node);
        }
        if (nodes != null)
        {
            graph.AddTransition(node.name, nodes);
        }
    }
    protected IFsmNode GetNode(string name)
    {
        for (int i = 0; i < fsmNodes.Count; i++)
        {
            if (fsmNodes[i].name == name)
                return fsmNodes[i];
        }
        return null;
    }
}
