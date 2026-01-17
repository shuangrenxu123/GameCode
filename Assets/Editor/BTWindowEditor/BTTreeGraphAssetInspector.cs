using System;
using System.Collections.Generic;
using System.Text;
using BT.Editor.RuntimeJson;
using BT.RuntimeSerialization;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor
{
    [CustomEditor(typeof(DefaultAsset))]
    class BTTreeGraphAssetInspector : UnityEditor.Editor
    {
        PropertyTree debugTree;
        GraphDebugActions debugActions;

        void OnEnable()
        {
            debugActions = new GraphDebugActions(LoadGraphForTarget);
            debugTree = PropertyTree.Create(debugActions);
        }

        void OnDisable()
        {
            debugTree = null;
            debugActions = null;
        }

        public override void OnInspectorGUI()
        {
            var assetPath = AssetDatabase.GetAssetPath(target)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith("." + BTTreeGraph.AssetExtension, StringComparison.OrdinalIgnoreCase))
            {
                DrawDefaultInspector();
                return;
            }

            var prevEnabled = GUI.enabled;
            GUI.enabled = true;

            EditorGUILayout.LabelField("BT Tree Graph", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Path", assetPath);

            var graphGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
            var defaultPath = BTTreeRuntimeJsonExporter.GetDefaultExportPath(assetPath);
            var resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime JSON Export", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newPath = EditorGUILayout.TextField(new GUIContent("Export Path (Assets/)"), configured ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
            {
                BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, newPath);
                configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
                resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select..."))
                {
                    var defaultName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    var savePath = EditorUtility.SaveFilePanelInProject(
                        "Select Runtime JSON export path",
                        string.IsNullOrEmpty(defaultName) ? "BTTree" : defaultName,
                        "json",
                        "Select a .json under Assets/ for TextAsset usage.",
                        string.IsNullOrEmpty(resolved) ? "Assets" : System.IO.Path.GetDirectoryName(resolved));

                    if (!string.IsNullOrEmpty(savePath))
                        BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, savePath);

                    configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
                    resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;
                }

                if (GUILayout.Button("Clear (Default)"))
                {
                    BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, string.Empty);
                    configured = null;
                    resolved = defaultPath;
                }

                if (GUILayout.Button("Export Once"))
                {
                    var graph = GraphDatabase.LoadGraph<BTTreeGraph>(assetPath);
                    if (graph == null)
                    {
                        Debug.LogError($"[BTTreeGraph] Failed to load graph: {assetPath}");
                    }
                    else if (!BTTreeRuntimeJsonExporter.TryExport(graph, resolved, skipIfUnchanged: true, out var wrote, out var error))
                    {
                        Debug.LogError($"[BTTreeGraph] Export Runtime JSON failed: {error}");
                    }
                    else if (wrote)
                    {
                        Debug.Log($"[BTTreeGraph] Exported Runtime JSON: {resolved}");
                    }
                }
            }

            EditorGUILayout.LabelField("Resolved Export Path", string.IsNullOrEmpty(resolved) ? "(Not Set)" : resolved);
            if (!string.IsNullOrEmpty(resolved) && !resolved.StartsWith("Assets/", StringComparison.Ordinal))
                EditorGUILayout.HelpBox("Export path must be under Assets/ to be used as a TextAsset.", MessageType.Warning);

            EditorGUILayout.Space();
            DrawDefaultInspector();

            if (debugTree != null)
            {
                EditorGUILayout.Space();
                debugActions.AssetPath = assetPath;
                debugTree.UpdateTree();
                debugTree.Draw(false);
            }

            GUI.enabled = prevEnabled;
        }

        BTTreeGraph LoadGraphForTarget()
        {
            if (string.IsNullOrEmpty(debugActions?.AssetPath))
                return null;

            return GraphDatabase.LoadGraph<BTTreeGraph>(debugActions.AssetPath);
        }

        sealed class GraphDebugActions
        {
            readonly Func<BTTreeGraph> graphProvider;

            public string AssetPath { get; set; }

            public GraphDebugActions(Func<BTTreeGraph> graphProvider)
            {
                this.graphProvider = graphProvider;
            }

            [Button("Log Node Structure")]
            void LogRuntimeTreeStructure()
            {
                var graph = graphProvider?.Invoke();
                if (graph == null)
                {
                    Debug.LogError("[BTTreeGraph] Failed to load graph; cannot log node structure.");
                    return;
                }

                BTTreeRuntimeJson dto;
                try
                {
                    dto = BTTreeRuntimeJsonExporter.BuildRuntimeJson(graph);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BTTreeGraph] Build runtime JSON failed: {e.Message}");
                    return;
                }

                var text = BuildTreeText(dto);
                Debug.Log(text);
            }

            static string BuildTreeText(BTTreeRuntimeJson dto)
            {
                if (dto == null || dto.nodes == null || dto.nodes.Count == 0)
                    return "[BTTreeGraph] Runtime node structure is empty.";

                var nodesById = new Dictionary<string, BTNodeRuntimeJson>(StringComparer.Ordinal);
                for (var i = 0; i < dto.nodes.Count; i++)
                {
                    var node = dto.nodes[i];
                    if (node == null || string.IsNullOrEmpty(node.id))
                        continue;
                    nodesById[node.id] = node;
                }

                if (string.IsNullOrEmpty(dto.rootId) || !nodesById.ContainsKey(dto.rootId))
                    return "[BTTreeGraph] No valid rootId; cannot build structure output.";

                var sb = new StringBuilder();
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var reachable = new HashSet<string>(StringComparer.Ordinal);

                AppendNode(dto.rootId, 0);

                if (reachable.Count < nodesById.Count)
                {
                    sb.AppendLine();
                    sb.AppendLine("[BTTreeGraph] Nodes not connected to root:");
                    foreach (var id in nodesById.Keys)
                    {
                        if (!reachable.Contains(id))
                        {
                            sb.Append(" - ").Append(nodesById[id]?.typeId ?? "Unknown")
                                .Append(" (").Append(id).Append(')')
                                .AppendLine();
                        }
                    }
                }

                return sb.ToString();

                void AppendNode(string id, int indent)
                {
                    if (!nodesById.TryGetValue(id, out var node))
                        return;

                    reachable.Add(id);
                    sb.Append(' ', indent * 2);
                    sb.Append(node.typeId ?? "Unknown");

                    if (node.args != null && node.args.Count > 0)
                    {
                        sb.Append(" [");
                        for (var i = 0; i < node.args.Count; i++)
                        {
                            var arg = node.args[i];
                            if (arg == null) continue;
                            if (i > 0) sb.Append(", ");
                            sb.Append(arg.name).Append('=').Append(arg.value);
                        }
                        sb.Append(']');
                    }

                    sb.AppendLine();

                    if (node.children == null || node.children.Count == 0)
                        return;

                    if (!visited.Add(id))
                    {
                        sb.Append(' ', (indent + 1) * 2);
                        sb.AppendLine("(Cycle detected, stop expanding)");
                        return;
                    }

                    foreach (var childId in node.children)
                    {
                        if (!string.IsNullOrEmpty(childId))
                            AppendNode(childId, indent + 1);
                    }
                }
            }
        }
    }
}
