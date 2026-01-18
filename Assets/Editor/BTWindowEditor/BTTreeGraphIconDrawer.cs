using UnityEditor;
using UnityEngine;

namespace BT.Editor
{
    [InitializeOnLoad]
    static class BTTreeGraphIconDrawer
    {
        const string IconAssetPath = "Assets/Editor//testIcon.png";

        static Texture2D icon;

        static BTTreeGraphIconDrawer()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconAssetPath);
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (icon == null)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !path.EndsWith("." + BTTreeGraph.AssetExtension))
                return;

            var isGridView = selectionRect.height > 20f;
            var size = isGridView
                ? Mathf.Min(selectionRect.width, selectionRect.height) * 0.8f
                : 16f;
            if (size <= 0f)
                return;

            var x = selectionRect.x + (selectionRect.width - size) * 0.5f;
            var y = selectionRect.y + (isGridView ? 6f : 1f);
            var iconRect = new Rect(x, y, size, size);
            var background = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.76f, 0.76f, 0.76f, 1f);
            EditorGUI.DrawRect(iconRect, background);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }
    }
}
