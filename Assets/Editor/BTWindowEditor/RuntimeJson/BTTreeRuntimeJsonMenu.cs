using System;
using System.IO;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.RuntimeJson
{
    static class BTTreeRuntimeJsonMenu
    {
        const string MenuRoot = "Assets/BT Tree Graph/";

        [MenuItem(MenuRoot + "Set Runtime JSON Export Path...", true)]
        static bool SetExportPath_Validate()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith("." + BTTreeGraph.AssetExtension, StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem(MenuRoot + "Set Runtime JSON Export Path...")]
        static void SetExportPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
                return;

            var graphGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var defaultName = Path.GetFileNameWithoutExtension(assetPath);
            var configured = BTTreeRuntimeJsonExportSettings.instance.GetExportAssetPath(graphGuid);
            var defaultPath = BTTreeRuntimeJsonExporter.GetDefaultExportPath(assetPath);
            var resolved = string.IsNullOrEmpty(configured) ? defaultPath : configured;

            var savePath = EditorUtility.SaveFilePanelInProject(
                "选择 Runtime JSON 导出路径",
                string.IsNullOrEmpty(defaultName) ? "BTTree" : defaultName,
                "json",
                "请选择导出到 Assets/ 下的 .json（用于 TextAsset 引用）",
                string.IsNullOrEmpty(resolved) ? "Assets" : Path.GetDirectoryName(resolved));

            if (string.IsNullOrEmpty(savePath))
                return;

            BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, savePath);
        }

        [MenuItem(MenuRoot + "Clear Runtime JSON Export Path (Use Default)", true)]
        static bool ClearExportPath_Validate() => SetExportPath_Validate();

        [MenuItem(MenuRoot + "Clear Runtime JSON Export Path (Use Default)")]
        static void ClearExportPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
                return;

            var graphGuid = AssetDatabase.AssetPathToGUID(assetPath);
            BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, string.Empty);
        }

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

            var defaultName = Path.GetFileNameWithoutExtension(assetPath);
            var savePath = EditorUtility.SaveFilePanelInProject(
                "Export Runtime BT JSON",
                string.IsNullOrEmpty(defaultName) ? "BTTree" : defaultName,
                "json",
                "Choose a location to save the runtime JSON (under Assets/).");

            if (string.IsNullOrEmpty(savePath))
                return;

            var graphGuid = AssetDatabase.AssetPathToGUID(assetPath);
            BTTreeRuntimeJsonExportSettings.instance.SetExportAssetPath(graphGuid, savePath);

            if (!BTTreeRuntimeJsonExporter.TryExport(graph, savePath, skipIfUnchanged: true, out var wrote, out var error))
            {
                EditorUtility.DisplayDialog("BT Tree Graph", $"Failed to export runtime JSON.\n{error}", "OK");
                return;
            }

            if (wrote)
                Debug.Log($"[BTTreeRuntimeJson] Exported: {savePath}");
        }
    }
}
