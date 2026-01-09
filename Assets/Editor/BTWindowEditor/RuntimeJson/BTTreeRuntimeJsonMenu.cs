using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BT.RuntimeSerialization;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    static class BTTreeRuntimeJsonMenu
    {
        const string MenuRoot = "Assets/BT Tree Graph/";

        [MenuItem(MenuRoot + "Export Runtime JSON", true)]
        static bool Export_Validate()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith("." + BTTreeGraph.AssetExtension, StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem(MenuRoot + "Export Runtime JSON")]
        static void Export()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
                return;

            var graph = GraphDatabase.LoadGraph<BTTreeGraph>(assetPath);
            if (graph == null)
            {
                EditorUtility.DisplayDialog("BT Tree Graph", $"Failed to load graph at '{assetPath}'.", "OK");
                return;
            }

            var dto = BuildRuntimeJson(graph);
            var json = Utf8Json.JsonSerializer.ToJsonString(dto);

            var defaultName = Path.GetFileNameWithoutExtension(assetPath);
            var savePath = EditorUtility.SaveFilePanelInProject("Export Runtime BT JSON", defaultName, "json", "Choose a location to save the runtime JSON.");
            if (string.IsNullOrEmpty(savePath))
                return;

            File.WriteAllText(savePath, json);
            AssetDatabase.Refresh();
        }

        static BTTreeRuntimeJson BuildRuntimeJson(BTTreeGraph graph)
        {
            var nodes = graph.GetNodes().OfType<IBTRuntimeJsonNode>().ToList();
            var rootNode = graph.GetNodes().OfType<BTEditorRootNode>().FirstOrDefault();

            var dto = new BTTreeRuntimeJson
            {
                nodes = new List<BTNodeRuntimeJson>(nodes.Count),
            };

            var nodeOrder = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < nodes.Count; i++)
            {
                if (!string.IsNullOrEmpty(nodes[i].NodeId))
                    nodeOrder[nodes[i].NodeId] = i;
            }

            var nodeIdHasParent = new HashSet<string>(StringComparer.Ordinal);

            foreach (var node in nodes)
            {
                var nodeDto = new BTNodeRuntimeJson
                {
                    id = node.NodeId,
                    typeId = node.RuntimeTypeId,
                    args = node.CollectArgs() ?? new List<BTArgJson>(),
                    children = new List<string>(),
                };

                foreach (var childId in CollectChildren(node, nodeOrder))
                {
                    if (string.IsNullOrEmpty(childId))
                        continue;
                    nodeDto.children.Add(childId);
                    nodeIdHasParent.Add(childId);
                }

                dto.nodes.Add(nodeDto);
            }

            dto.rootId = ResolveRootId(rootNode, nodeOrder) ??
                         dto.nodes.FirstOrDefault(n => !string.IsNullOrEmpty(n.id) && !nodeIdHasParent.Contains(n.id))?.id;
            return dto;
        }

        static string ResolveRootId(BTEditorRootNode rootNode, Dictionary<string, int> nodeOrder)
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
                return runtime.NodeId;

            return null;
        }

        struct ChildSortInfo
        {
            public string id;
            public Vector2 position;
            public int fallbackOrder;
            public int fallbackIndex;
        }

        static bool TryGetNodePosition(IBTRuntimeJsonNode runtimeNode, out Vector2 position)
        {
            position = default;

            if (runtimeNode is not Unity.GraphToolkit.Editor.Node node)
                return false;

            var implField = typeof(Unity.GraphToolkit.Editor.Node).GetField("m_Implementation", BindingFlags.Instance | BindingFlags.NonPublic);
            var impl = implField?.GetValue(node);
            if (impl == null)
                return false;

            var implType = impl.GetType();
            var posProp = implType.GetProperty("Position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (posProp != null && posProp.PropertyType == typeof(Vector2))
            {
                position = (Vector2)posProp.GetValue(impl);
                return true;
            }

            var posField = implType.GetField("m_Position", BindingFlags.Instance | BindingFlags.NonPublic);
            if (posField != null && posField.FieldType == typeof(Vector2))
            {
                position = (Vector2)posField.GetValue(impl);
                return true;
            }

            return false;
        }

        static IEnumerable<string> CollectChildren(IBTRuntimeJsonNode node, Dictionary<string, int> nodeOrder)
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
                    yield return child.NodeId;
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

                var children = new List<ChildSortInfo>(connected.Count);
                for (var i = 0; i < connected.Count; i++)
                {
                    var childNode = connected[i].GetNode();
                    if (childNode is not IBTRuntimeJsonNode child || string.IsNullOrEmpty(child.NodeId))
                        continue;

                    var hasPos = TryGetNodePosition(child, out var pos);
                    children.Add(new ChildSortInfo
                    {
                        id = child.NodeId,
                        // 需求：顺序控制节点的子节点顺序按“从上到下”，即 y 越小越靠前；y 相同则 x 越大越靠后（x 越小越靠前）。
                        // 如果拿不到位置，就把它排到最后，并用创建/枚举顺序做兜底以保证稳定性。
                        position = hasPos ? pos : new Vector2(float.PositiveInfinity, float.PositiveInfinity),
                        fallbackOrder = nodeOrder.TryGetValue(child.NodeId, out var idx) ? idx : int.MaxValue,
                        fallbackIndex = i,
                    });
                }

                foreach (var info in children
                             .OrderBy(c => c.position.y)
                             .ThenBy(c => c.position.x)
                             .ThenBy(c => c.fallbackOrder)
                             .ThenBy(c => c.fallbackIndex)
                             .ThenBy(c => c.id, StringComparer.Ordinal))
                    yield return info.id;
            }
        }
    }
}
