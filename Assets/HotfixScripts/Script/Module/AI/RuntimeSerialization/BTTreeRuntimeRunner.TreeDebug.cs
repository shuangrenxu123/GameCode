using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BT;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BT.RuntimeSerialization
{
    public sealed partial class BTTreeRuntimeRunner
    {
        static readonly BindingFlags TreeDebugBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Button("输出当前树结构", ButtonSizes.Medium)]
        [PropertyOrder(100)]
        void DumpCurrentTreeStructure()
        {
            if (root == null)
            {
                Debug.LogWarning("BTTreeRuntimeRunner: 当前没有已构建的行为树，请先构建后再输出树结构。", this);
                return;
            }

            var builder = new StringBuilder(256);
            builder.Append("BTTreeRuntimeRunner 树结构");
            if (runtimeJson != null)
            {
                builder.Append(" [");
                builder.Append(runtimeJson.name);
                builder.Append(']');
            }

            builder.AppendLine();
            builder.Append("GameObject: ");
            builder.AppendLine(name);
            builder.Append("LastResult: ");
            builder.AppendLine(lastResult?.ToString() ?? "None");

            var visited = new HashSet<BTNode>();
            AppendTreeNode(builder, root, string.Empty, true, visited);

            Debug.Log(builder.ToString(), this);
        }

        static void AppendTreeNode(
            StringBuilder builder,
            BTNode node,
            string indent,
            bool isLast,
            HashSet<BTNode> visited)
        {
            builder.Append(indent);
            builder.Append(isLast ? "`- " : "|- ");

            if (node == null)
            {
                builder.AppendLine("<null>");
                return;
            }

            builder.AppendLine(GetNodeDisplayName(node));

            if (!visited.Add(node))
            {
                builder.Append(indent);
                builder.Append(isLast ? "   " : "|  ");
                builder.AppendLine("`- <检测到重复引用，已停止继续展开>");
                return;
            }

            var children = CollectChildNodes(node);
            if (children.Count == 0)
                return;

            var childIndent = indent + (isLast ? "   " : "|  ");
            for (var i = 0; i < children.Count; i++)
            {
                AppendTreeNode(builder, children[i], childIndent, i == children.Count - 1, visited);
            }
        }

        static string GetNodeDisplayName(BTNode node)
        {
            var type = node.GetType();
            return string.IsNullOrEmpty(type.FullName) ? type.Name : type.FullName;
        }

        static List<BTNode> CollectChildNodes(BTNode node)
        {
            var result = new List<BTNode>();
            var dedup = new HashSet<BTNode>();

            void AddChild(BTNode child)
            {
                if (child != null && dedup.Add(child))
                    result.Add(child);
            }

            if (node is BTComposite composite)
            {
                var children = composite.children;
                for (var i = 0; i < children.Count; i++)
                    AddChild(children[i]);
                return result;
            }

            if (node is BTDecorator decorator)
            {
                AddChild(decorator.child);
                return result;
            }

            var type = node.GetType();
            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(TreeDebugBindingFlags | BindingFlags.DeclaredOnly);
                for (var i = 0; i < fields.Length; i++)
                {
                    var value = fields[i].GetValue(node);
                    CollectChildrenFromValue(value, AddChild);
                }

                type = type.BaseType;
            }

            return result;
        }

        static void CollectChildrenFromValue(object value, System.Action<BTNode> addChild)
        {
            if (value == null)
                return;

            if (value is BTNode childNode)
            {
                addChild(childNode);
                return;
            }

            if (value is string || value is not IEnumerable enumerable)
                return;

            foreach (var item in enumerable)
            {
                if (item is BTNode node)
                    addChild(node);
            }
        }
    }
}
