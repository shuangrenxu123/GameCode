using DialogueEdtior;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class GraphViewUtility
{
    private DialogueGraphView targetGraphView;
    private DialogueContainer container; 
    private List<Edge> Edges => targetGraphView.edges.ToList();
    /// <summary>
    /// 获取当前view中所有的nodes并转化为dialogueNode
    /// </summary>
    private List<DialogueNode> nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    public static GraphViewUtility GetInstance(DialogueGraphView tar)
    {
        return new GraphViewUtility() { targetGraphView = tar };
    }

    public void SaveData(string fileName)
    {
        if (!Edges.Any())
        {
            Debug.Log("当前图中没有连线");
            return;
        }
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        //挑选出所有输入节点不为null的连线
        var connectPorts = Edges.Where(x => x.input.node !=null).ToArray();

        for (int i = 0; i < connectPorts.Length; i++)
        {
            var outputNode = connectPorts[i].output.node as DialogueNode;
            var inputNode = connectPorts[i].input.node as DialogueNode;

            //添加一个连接
            dialogueContainer.nodeLinks.Add(new NodeLInkData()
            {
                BaseNodeGuid =outputNode.GUID,
                PortName= connectPorts[i].output.portName,
                TargetNodeGuid=inputNode.GUID,
            });
        }

        foreach (var i in nodes.Where(node=>!node.EntryPoint))
        {
            dialogueContainer.NodeDatas.Add(new DialogueNodeData()
            {
                guid = i.GUID,
                dialogueText = i.DialogueText,
                position = i.GetPosition().position
            });
        }

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadData(string fileName) 
    {
        container = Resources.Load<DialogueContainer>(fileName);
        if (container == null)
        {
            Debug.LogError("没有找到文件");
            return;
        }
        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var conn = container.nodeLinks.Where(x => x.BaseNodeGuid == nodes[i].GUID).ToList();
            for (int j = 0; j < conn.Count; j++)
            {
                var targetNodeguid = conn[j].TargetNodeGuid;
                var targetNode = nodes.First(x => x.GUID == targetNodeguid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(container.NodeDatas.First(x => x.guid == targetNodeguid).position, targetGraphView.NodeSize));

            }
        }
    }

    private void CreateNodes()
    {
        foreach (var nodedata in container.NodeDatas)
        {
            var tempNode = targetGraphView.CreatDialogueNode(nodedata.dialogueText);
            tempNode.GUID= nodedata.guid;
            targetGraphView.AddElement(tempNode);
            var nodePorts = container.nodeLinks.Where(x => x.BaseNodeGuid== nodedata.guid).ToList();
            nodePorts.ForEach(x => { targetGraphView.AddChoicePort(tempNode, x.PortName); });
        }
    }

    private void LinkNodes(Port output,Port input)
    {
        var tempEdge = new Edge()
        {
            output = output,
            input = input
        };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        targetGraphView.Add(tempEdge);
    }

    private void ClearGraph()
    {
        foreach (var node in nodes)
        {
            //Edges.Where(x => x.input.node == node).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));
            Edges.ForEach(e => targetGraphView.RemoveElement(e));
            targetGraphView.Remove(node);
        }
        //targetGraphView.Clear();
    }
}
