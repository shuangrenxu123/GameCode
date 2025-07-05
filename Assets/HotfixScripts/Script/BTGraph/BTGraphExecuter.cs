using BT;
using UnityEngine;
using XNode;

public class BTGraphExecuter : MonoBehaviour
{
    [SerializeField]
    NodeGraph graph;

    [SerializeField]
    bool activeUpdate;

    NormalBTTree tree;
    void Start()
    {
        tree = new();
        tree.SetNodeWithGraph(graph);
    }

    void Update()
    {
        if (activeUpdate)
        {
            ExecuteTree();
        }
    }
    public void ExecuteTree()
    {
        tree.Update();
    }

}
class NormalBTTree : BTTree
{
    public void SetNodeWithGraph(NodeGraph graph)
    {
        foreach (var node in graph.nodes)
        {
            if (node.GetType().Name == "RootNode")
            {
                var treeRoot = (node as BaseNode).ToRuntime();
                root = treeRoot;
                break;
            }
        }

    }
    public override void SetNode()
    {
    }

}
