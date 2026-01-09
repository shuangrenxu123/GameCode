using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

                var children = new List<string>(connected.Count);
                for (var i = 0; i < connected.Count; i++)
                {
                    var childNode = connected[i].GetNode();
                    if (childNode is IBTRuntimeJsonNode child && !string.IsNullOrEmpty(child.NodeId))
                        children.Add(child.NodeId);
                }

                foreach (var id in children.OrderBy(id => nodeOrder.TryGetValue(id, out var idx) ? idx : int.MaxValue))
                    yield return id;
            }
        }
    }
}
