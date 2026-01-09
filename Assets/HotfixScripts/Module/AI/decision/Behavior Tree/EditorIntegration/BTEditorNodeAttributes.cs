using System;

namespace BT.EditorIntegration
{
    public enum BTEditorNodeKind
    {
        Leaf = 0,
        Decorator = 1,
        Composite = 2,
    }

    public enum BTChildCapacity
    {
        None = 0,
        Single = 1,
        Multi = 2,
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BTEditorNodeAttribute : Attribute
    {
        public string Path { get; }
        public BTEditorNodeKind Kind { get; }
        public BTChildCapacity ChildCapacity { get; }

        public BTEditorNodeAttribute(string path, BTEditorNodeKind kind, BTChildCapacity childCapacity = BTChildCapacity.None)
        {
            Path = path;
            Kind = kind;
            ChildCapacity = childCapacity == BTChildCapacity.None
                ? kind == BTEditorNodeKind.Decorator ? BTChildCapacity.Single :
                  kind == BTEditorNodeKind.Composite ? BTChildCapacity.Multi :
                  BTChildCapacity.None
                : childCapacity;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    public sealed class BTEditorConstructorAttribute : Attribute
    {
    }
}
