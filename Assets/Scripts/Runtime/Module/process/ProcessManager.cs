using System.Collections.Generic;
using UnityEngine;

public class ProcessManager : ModuleSingleton<ProcessManager>, IModule
{
    IProcessNode currNode;
    List<IProcessNode> nodes;
    public void OnCreate(object createParam)
    {
        nodes = new List<IProcessNode>(10);
        currNode = new CheckProcess();
        nodes.Add(currNode);
    }

    public void OnUpdate()
    {
        currNode.Update();
    }
    public void ChangeNode<T>(T node) where T : IProcessNode
    {
        if (nodes.Contains(node))
        {
            currNode.Exit();
            currNode = node;
            currNode.Enter();
        }
        else
        {
            Debug.Log("节点不存在");
        }
    }
}
