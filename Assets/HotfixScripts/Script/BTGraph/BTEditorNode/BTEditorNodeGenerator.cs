// BTNodeGenerator.cs
#if UNITY_EDITOR
using System;
// BTNodeAttribute.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BT;
using UnityEditor;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// 标记需要生成编辑器节点类的行为树节点
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class BTNodeAttribute : Attribute
    {
        public enum NodeType
        {
            Composite,
            Decorator,
            Action
        }

        public NodeType Type { get; }
        public string MenuPath { get; }
        public string EditorClassName { get; }

        public BTNodeAttribute(NodeType type, string menuPath, string editorClassName = null)
        {
            Type = type;
            MenuPath = menuPath;
            EditorClassName = editorClassName;
        }
    }

    /// <summary>
    /// 标记需要在编辑器节点中显示的字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class BTPropertyAttribute : Attribute
    {
        public string DisplayName { get; }

        public BTPropertyAttribute(string displayName = null)
        {
            DisplayName = displayName;
        }
    }
}


public static class BTNodeGenerator
{
    private const string GENERATED_FILE = "Assets/BTEditor/Generated/BTGeneratedNodes.cs";
    private const string NAMESPACE = "BTEditor.Generated";

    [MenuItem("Tools/Behavior Tree/Generate Node Classes")]
    public static void GenerateNodeClasses()
    {
        // 确保目录存在
        var directory = Path.GetDirectoryName(GENERATED_FILE);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 扫描所有标记了BTNodeAttribute的类
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(typeof(BTNodeAttribute), false))
            .ToList();

        if (types.Count == 0)
        {
            Debug.LogWarning("未找到标记了BTNodeAttribute的类");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // 添加文件头部
        sb.AppendLine("// ================================================");
        sb.AppendLine("// 自动生成的行为树编辑器节点类 - 请勿手动修改");
        sb.AppendLine("// 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("// ================================================");
        sb.AppendLine();

        // 添加引用
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using XNode;");
        sb.AppendLine("using BT;");
        sb.AppendLine();

        // 添加命名空间
        sb.AppendLine($"namespace {NAMESPACE}");
        sb.AppendLine("{");

        // 为每个类型生成节点类
        foreach (var runtimeType in types)
        {
            GenerateNodeClass(sb, runtimeType);
        }

        sb.AppendLine("}"); // 结束命名空间

        // 写入文件
        File.WriteAllText(GENERATED_FILE, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"成功生成 {types.Count} 个行为树节点类到: {GENERATED_FILE}");
    }

    private static void GenerateNodeClass(StringBuilder sb, Type runtimeType)
    {
        var attribute = runtimeType.GetCustomAttribute<BTNodeAttribute>();
        string className = attribute.EditorClassName ?? runtimeType.Name.Replace("BT", "") + "Node";

        // 添加类注释
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// 自动生成的编辑器节点类 - 对应运行时类: {runtimeType.Name}");
        sb.AppendLine($"    /// </summary>");

        // 添加CreateNodeMenu属性
        sb.AppendLine($"    [CreateNodeMenu(\"{attribute.MenuPath}\")]");

        // 添加类声明
        string baseClass = attribute.Type switch
        {
            BTNodeAttribute.NodeType.Composite => "CompositeNode",
            BTNodeAttribute.NodeType.Decorator => "DecoratorNode",
            BTNodeAttribute.NodeType.Action => "ActionBaseNode",
            _ => "BaseNode"
        };

        sb.AppendLine($"    public class {className} : {baseClass}");
        sb.AppendLine("    {");

        // 添加序列化字段
        var fields = runtimeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsDefined(typeof(BTPropertyAttribute), false));

        foreach (var field in fields)
        {
            var propAttribute = field.GetCustomAttribute<BTPropertyAttribute>();
            string displayName = !string.IsNullOrEmpty(propAttribute.DisplayName) ?
                $"(\"{propAttribute.DisplayName}\")" : "";

            sb.AppendLine($"        [SerializeField]{displayName}");
            sb.AppendLine($"        {GetFieldTypeName(field.FieldType)} {field.Name};");
            sb.AppendLine();
        }

        // 添加ToRuntime方法
        sb.AppendLine("        public override BTNode ToRuntime()");
        sb.AppendLine("        {");

        switch (attribute.Type)
        {
            case BTNodeAttribute.NodeType.Composite:
                sb.AppendLine($"            var node = new {runtimeType.Name}();");
                sb.AppendLine("            AddNode(node);");
                sb.AppendLine("            return node;");
                break;

            case BTNodeAttribute.NodeType.Decorator:
                sb.AppendLine("            var child = GetChildNode();");
                sb.Append($"            return new {runtimeType.Name}(");

                // 构造参数：子节点 + 所有标记字段
                var ctorParams = new List<string> { "child" };
                ctorParams.AddRange(fields.Select(f => f.Name));
                sb.Append(string.Join(", ", ctorParams));
                sb.AppendLine(");");
                break;

            case BTNodeAttribute.NodeType.Action:
                sb.Append($"            return new {runtimeType.Name}(");
                sb.Append(string.Join(", ", fields.Select(f => f.Name)));
                sb.AppendLine(");");
                break;
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    private static string GetFieldTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        return type.Name;
    }
}
#endif
