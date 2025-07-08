// ================================================
// 自动生成的行为树编辑器节点类 - 请勿手动修改
// 生成时间: 2025-07-08 18:13:59
// ================================================

using BT;
using BTEditor.Test;
using UnityEngine;
using XNode;
using static DebugAction;
using static XNode.Node;


namespace BTEditor.Generated
{

    /// <summary>
    /// 自动生成的编辑器节点类 - 对应运行时类: DebugAction
    /// </summary>
    [CreateNodeMenu("Action/Log")]
    public class DebugActionEditor : ActionBaseNode
    {
        [SerializeField]
        string logInfo;

        [SerializeField]
        DebugActionType type;

        public override BTNode ToRuntime()
        {
            return new DebugAction(logInfo, type);
        }
    }

    /// <summary>
    /// 自动生成的编辑器节点类 - 对应运行时类: BTSequence
    /// </summary>
    [CreateNodeMenu("Test/BTSeq")]
    public class BTSequenceEditor : CompositeNode
    {
        public override BTNode ToRuntime()
        {
            var node = new BTSequence();
            AddNode(node);
            return node;
        }
    }
}
