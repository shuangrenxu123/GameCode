using System;
using BT.Editor.RuntimeJson;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor
{
    [CustomEditor(typeof(DefaultAsset))]
    class BTTreeGraphAssetInspector : UnityEditor.Editor
    {
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
            EditorGUILayout.LabelField("路径", assetPath);

            var graphGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
            var defaultPath = BTTreeRuntimeJsonExporter.GetDefaultExportPath(assetPath);
            var resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime JSON 导出", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newPath = EditorGUILayout.TextField(new GUIContent("导出路径(Assets内)"), configured ?? string.Empty);
            if (EditorGUI.EndChangeCheck())
            {
                BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, newPath);
                configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
                resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("选择..."))
                {
                    var defaultName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    var savePath = EditorUtility.SaveFilePanelInProject(
                        "选择 Runtime JSON 导出路径",
                        string.IsNullOrEmpty(defaultName) ? "BTTree" : defaultName,
                        "json",
                        "请选择导出到 Assets/ 下的 .json（用于 TextAsset 引用）",
                        string.IsNullOrEmpty(resolved) ? "Assets" : System.IO.Path.GetDirectoryName(resolved));

                    if (!string.IsNullOrEmpty(savePath))
                        BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, savePath);

                    configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
                    resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;
                }

                if (GUILayout.Button("清空(使用默认)"))
                {
                    BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, string.Empty);
                    configured = null;
                    resolved = defaultPath;
                }

                if (GUILayout.Button("导出一次"))
                {
                    var graph = GraphDatabase.LoadGraph<BTTreeGraph>(assetPath);
                    if (graph == null)
                    {
                        Debug.LogError($"[BTTreeGraph] 加载图失败：{assetPath}");
                    }
                    else if (!BTTreeRuntimeJsonExporter.TryExport(graph, resolved, skipIfUnchanged: true, out var wrote, out var error))
                    {
                        Debug.LogError($"[BTTreeGraph] 导出 Runtime JSON 失败：{error}");
                    }
                    else if (wrote)
                    {
                        Debug.Log($"[BTTreeGraph] 已导出 Runtime JSON：{resolved}");
                    }
                }
            }

            EditorGUILayout.LabelField("实际导出路径", string.IsNullOrEmpty(resolved) ? "(未设置)" : resolved);
            if (!string.IsNullOrEmpty(resolved) && !resolved.StartsWith("Assets/", StringComparison.Ordinal))
                EditorGUILayout.HelpBox("导出路径必须位于 Assets/ 下，否则无法作为 TextAsset 引用。", MessageType.Warning);

            EditorGUILayout.Space();
            DrawDefaultInspector();

            GUI.enabled = prevEnabled;
        }
    }
}
