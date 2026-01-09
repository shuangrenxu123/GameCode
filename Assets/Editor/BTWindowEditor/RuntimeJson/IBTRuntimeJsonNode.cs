using System.Collections.Generic;
using BT.EditorIntegration;
using BT.RuntimeSerialization;

namespace BT.Editor.RuntimeJson
{
    interface IBTRuntimeJsonNode
    {
        string NodeId { get; }
        string RuntimeTypeId { get; }
        BTEditorNodeKind Kind { get; }
        BTChildCapacity ChildCapacity { get; }
        List<BTArgJson> CollectArgs();
    }
}
