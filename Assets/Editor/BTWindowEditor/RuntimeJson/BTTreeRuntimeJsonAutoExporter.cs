using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    class BTTreeRuntimeJsonAutoExporter : AssetModificationProcessor
    {
        static readonly HashSet<string> PendingGraphAssetPaths = new(StringComparer.OrdinalIgnoreCase);
        static bool _scheduled;

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return paths;

            for (var i = 0; i < paths.Length; i++)
            {
                var p = paths[i];
                if (string.IsNullOrEmpty(p))
                    continue;

                if (p.EndsWith("." + BTTreeGraph.AssetExtension, StringComparison.OrdinalIgnoreCase))
                    PendingGraphAssetPaths.Add(p.Replace('\\', '/'));
            }

            if (PendingGraphAssetPaths.Count > 0 && !_scheduled)
            {
                _scheduled = true;
                EditorApplication.delayCall += ExportAfterSave;
            }

            return paths;
        }

        static void ExportAfterSave()
        {
            _scheduled = false;

            if (PendingGraphAssetPaths.Count == 0)
                return;

            var candidates = PendingGraphAssetPaths.ToArray();
            PendingGraphAssetPaths.Clear();

            var activePath = AssetDatabase.GetAssetPath(Selection.activeObject)?.Replace('\\', '/');
            var targetGraphPath = ResolveTargetGraphPath(candidates, activePath);
            if (string.IsNullOrEmpty(targetGraphPath))
                return;

            var graph = GraphDatabase.LoadGraph<BTTreeGraph>(targetGraphPath);
            if (graph == null)
                return;

            var graphGuid = AssetDatabase.AssetPathToGUID(targetGraphPath);
            var configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
            var exportPath = string.IsNullOrEmpty(configured)
                ? BTTreeRuntimeJsonExporter.GetDefaultExportPath(targetGraphPath)
                : configured;

            if (string.IsNullOrEmpty(exportPath))
            {
                Debug.LogWarning($"[BTTreeRuntimeJson] 自动导出跳过：未配置导出路径，且无法推导默认路径。Graph='{targetGraphPath}'");
                return;
            }

            if (!BTTreeRuntimeJsonExporter.TryExport(graph, exportPath, skipIfUnchanged: true, out var wrote, out var error))
            {
                Debug.LogError($"[BTTreeRuntimeJson] 自动导出失败：Graph='{targetGraphPath}' Export='{exportPath}'\n{error}");
                return;
            }

            if (wrote)
                Debug.Log($"[BTTreeRuntimeJson] 自动导出：{exportPath}");
        }

        static string ResolveTargetGraphPath(string[] candidates, string activePath)
        {
            if (candidates == null || candidates.Length == 0)
                return null;

            if (!string.IsNullOrEmpty(activePath) &&
                activePath.EndsWith("." + BTTreeGraph.AssetExtension, StringComparison.OrdinalIgnoreCase))
            {
                for (var i = 0; i < candidates.Length; i++)
                {
                    if (string.Equals(candidates[i], activePath, StringComparison.OrdinalIgnoreCase))
                        return candidates[i];
                }
            }

            if (candidates.Length == 1)
                return candidates[0];

            Array.Sort(candidates, StringComparer.OrdinalIgnoreCase);
            Debug.LogWarning("[BTTreeRuntimeJson] 本次保存包含多个 BTTreeGraph，且未能确定当前编辑的图；为避免误覆盖，已跳过自动导出。");
            return null;
        }
    }
}
