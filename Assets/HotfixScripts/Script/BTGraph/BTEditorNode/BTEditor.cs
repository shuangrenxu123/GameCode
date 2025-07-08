using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BT;
using UnityEngine;
using XNode;
using XNodeEditor;
using static DebugAction;
using static XNodeEditor.NodeEditor;

namespace BTEditor.Test
{

    [CreateAssetMenu]

    public class BTEditor : NodeGraph
    {
        void Awake()
        {
            var root = AddNode<RootNode>();
            root.name = "root";
        }
    }
    [HideInInspector]
    class RootNode : BaseNode
    {
        [Output(connectionType = ConnectionType.Override)]
        public BaseNode child;

        public override BTNode ToRuntime()
        {
            var currentNode = new BTSequence();
            foreach (var outPort in Ports)
            {
                if (outPort.IsInput)
                {
                    continue;
                }
                var child = (outPort.Connection.node as BaseNode).ToRuntime();
                currentNode.AddChild(child);
            }
            return currentNode;
        }
    }
    [CreateNodeMenu("调试/DebugAction")]
    public class DebugActionNode : ActionBaseNode
    {
        [SerializeField]
        string info;
        [SerializeField]
        DebugActionType type;
        public override BTNode ToRuntime()
        {
            return new DebugAction(info, type);
        }
    }


    public abstract class ActionBaseNode : BaseNode
    {
        [Input(connectionType = ConnectionType.Override)]
        public CompositeNode baseNode;
    }
    public abstract class DecoratorNode : BaseNode
    {
        [Input(connectionType = ConnectionType.Override)]
        public BaseNode baseNode;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNode childNode;
        protected BTNode GetChildNode()
        {
            var connectedNode = Ports
                  .First(p => p.IsOutput)
                  .GetConnections()
                  .First()
                  .node;

            var child = (connectedNode as BaseNode).ToRuntime();
            return child;
        }
    }
    public abstract class CompositeNode : BaseNode
    {
        [Input(connectionType = ConnectionType.Override)]
        public BaseNode baseNode;
        [Output]
        public BaseNode chiles;

        protected void AddNode<T>(T baseNode) where T : BTComposite
        {
            var connectedNodes = Ports
                .First(p => p.IsOutput)
                .GetConnections()
                .Select(p => p.node)
                .OrderBy(node => node.position.y)
                .ToList();// 按Y坐标排序（从顶到底）

            foreach (var outNode in connectedNodes)
            {
                var child = (outNode as BaseNode).ToRuntime();
                baseNode.AddChild(child);
            }
        }

    }

    public abstract class BaseNode : Node
    {
        public abstract BTNode ToRuntime();
    }

    [CreateNodeMenu("组合节点/SequenceNode")]
    public class SequenceNode : CompositeNode
    {
        public override BTNode ToRuntime()
        {
            var currentNode = new BTSequence();
            AddNode(currentNode);
            return currentNode;
        }
    }

    [CreateNodeMenu("组合节点/Selector")]
    public class SelectorNode : CompositeNode
    {
        public override BTNode ToRuntime()
        {
            var currentNode = new BTSelector();
            AddNode(currentNode);
            return currentNode;
        }
    }

    [CreateNodeMenu("组合节点/ParallelNode")]
    public class ParallelNode : CompositeNode
    {
        public ParallelType type;
        public override BTNode ToRuntime()
        {
            var currentNode = new BTParallel(type);
            AddNode(currentNode);
            return currentNode;
        }
    }

    [CreateNodeMenu("修饰节点/通用/TimerNode")]
    public class WaitNode : DecoratorNode
    {
        [SerializeField]
        float waitTime;

        public override BTNode ToRuntime()
        {
            var child = GetChildNode();
            return new BTTimer(waitTime, child);
        }
    }
}