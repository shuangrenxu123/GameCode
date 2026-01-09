using System;
using BT.Editor.Internal;
using Unity.GraphToolkit.Editor;

namespace BT.Editor.RuntimeJson
{
    [Serializable]
    [UseWithGraph(typeof(BTTreeGraph))]
    class BTEditorRootNode : Node
    {
        public const string ChildPortId = "Child";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var output = context.AddOutputPort(ChildPortId).WithDisplayName("Root").Build();
            GraphToolkitPortCapacityUtil.TryForceSingleCapacity(output);
        }
    }
}

