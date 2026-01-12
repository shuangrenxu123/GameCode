using System;
using Unity.GraphToolkit.Editor;

namespace BT.Editor.Internal
{
    /// <summary>
    /// GraphToolkit 的黑板变量类型列表来自 GraphModelImp.SupportedTypes，
    /// 它是通过扫描“该 Graph 支持的所有 Node 的端口类型”推导出来的。
    ///
    /// 行为树节点端口几乎全是 ExecutionFlow，因此默认情况下黑板“+”里可能看不到 int/float/bool/string。
    /// 该节点只用于向 SupportedTypes 注入黑板需要的基础类型；无需在图中实际创建或连接。
    /// </summary>
    [Serializable]
    [UseWithGraph(typeof(BTTreeGraph))]
    class BTBlackboardSupportedTypesNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>("__bb_int").Build();
            context.AddInputPort<float>("__bb_float").Build();
            context.AddInputPort<bool>("__bb_bool").Build();
            context.AddInputPort<string>("__bb_string").Build();
        }
    }
}

