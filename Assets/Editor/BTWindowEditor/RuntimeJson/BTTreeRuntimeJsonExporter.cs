using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BT.Editor;
using BT.RuntimeSerialization;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    static class BTTreeRuntimeJsonExporter
    {
        static readonly System.Text.Encoding Utf8NoBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        static readonly FieldInfo NodeImplementationField = typeof(Node).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly Dictionary<Type, PropertyInfo> NodePositionPropertyByType = new();

        public static string GetDefaultExportPath(string graphAssetPath)
        {
            if (string.IsNullOrEmpty(graphAssetPath))
                return null;

            var dir = Path.GetDirectoryName(graphAssetPath)?.Replace('\\', '/');
            var name = Path.GetFileNameWithoutExtension(graphAssetPath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name))
                return null;

            return $"{dir}/{name}.runtime.json";
        }

        public static bool TryExport(BTTreeGraph graph, string exportAssetPath, bool skipIfUnchanged, out bool wroteFile, out string error)
        {
            wroteFile = false;
            error = null;

            if (graph == null)
            {
                error = "graph is null.";
                return false;
            }

            if (string.IsNullOrEmpty(exportAssetPath))
            {
                error = "exportAssetPath is empty.";
                return false;
            }

            exportAssetPath = exportAssetPath.Replace('\\', '/');
            if (!exportAssetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                error = $"exportAssetPath must be under Assets/: '{exportAssetPath}'";
                return false;
            }

            if (!exportAssetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                error = $"exportAssetPath must end with .json: '{exportAssetPath}'";
                return false;
            }

            try
            {
                var dto = BuildRuntimeJson(graph);
                var json = Utf8Json.JsonSerializer.ToJsonString(dto);

                var projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(projectRoot))
                {
                    error = "Failed to resolve Unity project root path.";
                    return false;
                }

                var exportFullPath = Path.GetFullPath(Path.Combine(projectRoot, exportAssetPath));

                if (skipIfUnchanged && File.Exists(exportFullPath))
                {
                    var existing = File.ReadAllText(exportFullPath, Utf8NoBom);
                    if (string.Equals(existing, json, StringComparison.Ordinal))
                        return true;
                }

                var dir = Path.GetDirectoryName(exportFullPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(exportFullPath, json, Utf8NoBom);
                wroteFile = true;

                AssetDatabase.ImportAsset(exportAssetPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[BTTreeRuntimeJson] 已生成：{exportAssetPath}");
                return true;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            }
        }

        internal static BTTreeRuntimeJson BuildRuntimeJson(BTTreeGraph graph)
        {
            var nodes = graph.GetNodes().OfType<IBTRuntimeJsonNode>().ToList();
            var rootNode = graph.GetNodes().OfType<BTEditorRootNode>().FirstOrDefault();

            var dto = new BTTreeRuntimeJson
            {
                nodes = new List<BTNodeRuntimeJson>(nodes.Count),
            };

            ExportBlackboard(graph, dto);

            var exportIdByNode = BuildExportIdMap(nodes);
            string GetExportId(IBTRuntimeJsonNode n) => exportIdByNode.TryGetValue(n, out var id) ? id : n?.NodeId;

            var nodeOrder = new Dictionary<IBTRuntimeJsonNode, int>();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null)
                    nodeOrder[nodes[i]] = i;
            }

            var nodeIdHasParent = new HashSet<string>(StringComparer.Ordinal);

            foreach (var node in nodes)
            {
                var exportId = GetExportId(node);
                var collectedChildren = CollectChildren(node, nodeOrder).ToList();
                if (node.Kind == BT.EditorIntegration.BTEditorNodeKind.Decorator && collectedChildren.Count == 0)
                {
                    Debug.LogError($"[BTTreeRuntimeJson] Decorator 节点未连接子节点，已停止导出。NodeId='{exportId}', TypeId='{node.RuntimeTypeId}'");
                    throw new InvalidOperationException("Decorator node must have a child when exporting runtime JSON.");
                }

                var nodeDto = new BTNodeRuntimeJson
                {
                    id = exportId,
                    typeId = node.RuntimeTypeId,
                    args = node.CollectArgs() ?? new List<BTArgJson>(),
                    children = new List<string>(),
                };

                foreach (var child in collectedChildren)
                {
                    var childId = GetExportId(child);
                    if (string.IsNullOrEmpty(childId))
                        continue;
                    nodeDto.children.Add(childId);
                    nodeIdHasParent.Add(childId);
                }

                dto.nodes.Add(nodeDto);
            }

            dto.rootId = ResolveRootId(rootNode, GetExportId) ??
                         dto.nodes.FirstOrDefault(n => !string.IsNullOrEmpty(n.id) && !nodeIdHasParent.Contains(n.id))?.id;
            return dto;
        }

        static Dictionary<IBTRuntimeJsonNode, string> BuildExportIdMap(List<IBTRuntimeJsonNode> nodes)
        {
            var map = new Dictionary<IBTRuntimeJsonNode, string>();
            var used = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                    continue;

                var id = node.NodeId;
                if (string.IsNullOrEmpty(id) || !used.Add(id))
                {
                    id = Guid.NewGuid().ToString("N");
                    used.Add(id);
                }

                map[node] = id;
            }

            return map;
        }

        static void ExportBlackboard(BTTreeGraph graph, BTTreeRuntimeJson dto)
        {
            dto.blackboard ??= new List<BTBlackboardEntryJson>();
            dto.blackboard.Clear();

            foreach (var variable in graph.GetVariables())
            {
                if (variable == null)
                    continue;

                var key = variable.name;
                if (string.IsNullOrEmpty(key))
                    continue;

                var type = variable.dataType;
                if (type == typeof(int))
                {
                    variable.TryGetDefaultValue(out int v);
                    dto.blackboard.Add(new BTBlackboardEntryJson
                    {
                        key = key,
                        type = BTBlackboardValueType.Int,
                        value = v.ToString(CultureInfo.InvariantCulture),
                    });
                }
                else if (type == typeof(float))
                {
                    variable.TryGetDefaultValue(out float v);
                    dto.blackboard.Add(new BTBlackboardEntryJson
                    {
                        key = key,
                        type = BTBlackboardValueType.Float,
                        value = v.ToString(CultureInfo.InvariantCulture),
                    });
                }
                else if (type == typeof(bool))
                {
                    variable.TryGetDefaultValue(out bool v);
                    dto.blackboard.Add(new BTBlackboardEntryJson
                    {
                        key = key,
                        type = BTBlackboardValueType.Bool,
                        value = v ? "true" : "false",
                    });
                }
                else if (type == typeof(string))
                {
                    variable.TryGetDefaultValue(out string v);
                    dto.blackboard.Add(new BTBlackboardEntryJson
                    {
                        key = key,
                        type = BTBlackboardValueType.String,
                        value = v ?? string.Empty,
                    });
                }
            }
        }

        static string ResolveRootId(BTEditorRootNode rootNode, Func<IBTRuntimeJsonNode, string> getId)
        {
            if (rootNode == null)
                return null;

            var port = rootNode.GetOutputPortByName(BTEditorRootNode.ChildPortId);
            if (port == null)
                return null;

            var connected = new List<IPort>();
            port.GetConnectedPorts(connected);
            if (connected.Count == 0)
                return null;

            var childNode = connected[0].GetNode();
            if (childNode is IBTRuntimeJsonNode runtime)
                return getId?.Invoke(runtime) ?? runtime.NodeId;

            return null;
        }

        static IEnumerable<IBTRuntimeJsonNode> CollectChildren(IBTRuntimeJsonNode node, Dictionary<IBTRuntimeJsonNode, int> nodeOrder)
        {
            if (node is not INode graphNode)
                yield break;

            var connected = new List<IPort>();
            if (node.ChildCapacity == BT.EditorIntegration.BTChildCapacity.Single)
            {
                var port = graphNode.GetOutputPortByName("Child");
                if (port == null)
                    yield break;

                connected.Clear();
                port.GetConnectedPorts(connected);
                if (connected.Count == 0)
                    yield break;

                var childNode = connected[0].GetNode();
                if (childNode is IBTRuntimeJsonNode child)
                    yield return child;
                yield break;
            }

            if (node.ChildCapacity == BT.EditorIntegration.BTChildCapacity.Multi)
            {
                var port = graphNode.GetOutputPortByName("Children");
                if (port == null)
                    yield break;

                connected.Clear();
                port.GetConnectedPorts(connected);
                if (connected.Count == 0)
                    yield break;

                var children = new List<(IBTRuntimeJsonNode node, int order, int index, string id, float y, float x, bool hasPosition)>(connected.Count);
                for (var i = 0; i < connected.Count; i++)
                {
                    var childNode = connected[i].GetNode();
                    if (childNode is not IBTRuntimeJsonNode child)
                        continue;

                    var order = nodeOrder.TryGetValue(child, out var idx) ? idx : int.MaxValue;
                    var hasPosition = TryGetNodePosition(child, out var position);
                    var y = hasPosition ? position.y : float.MaxValue;
                    var x = hasPosition ? position.x : float.MaxValue;
                    children.Add((child, order, i, child.NodeId, y, x, hasPosition));
                }

                foreach (var info in children
                             .OrderBy(c => c.hasPosition ? 0 : 1)
                             .ThenBy(c => c.y)
                             .ThenBy(c => c.x)
                             .ThenBy(c => c.order)
                             .ThenBy(c => c.index)
                             .ThenBy(c => c.id, StringComparer.Ordinal))
                    yield return info.node;
            }
        }

        static bool TryGetNodePosition(IBTRuntimeJsonNode node, out Vector2 position)
        {
            position = Vector2.zero;
            if (node == null)
                return false;

            try
            {
                var implementation = NodeImplementationField?.GetValue(node);
                if (implementation == null)
                    return false;

                var type = implementation.GetType();
                if (!NodePositionPropertyByType.TryGetValue(type, out var property))
                {
                    property = type.GetProperty("Position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    NodePositionPropertyByType[type] = property;
                }

                if (property == null)
                    return false;

                if (property.GetValue(implementation) is Vector2 value)
                {
                    position = value;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
