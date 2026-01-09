using System;

namespace BT.EditorIntegration
{
    public enum BTEditorNodeKind
    {
        Action = 0,
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

    /// <summary>
    /// 标记运行时节点的字段/属性需要在编辑器节点里暴露出来，并在导出 RuntimeJson 时作为 args 写入。
    /// 约束：成员必须可被生成的工厂代码赋值（public 或 internal 且可写）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class BTEditorExposeAttribute : Attribute
    {
        /// <summary>
        /// 导出到 JSON args 的参数名；为空则使用成员名。
        /// </summary>
        public string Name { get; }

        public BTEditorExposeAttribute(string name = null)
        {
            Name = name;
        }
    }
}
