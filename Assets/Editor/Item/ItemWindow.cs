using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemWindow : EditorWindow
{

    [MenuItem("Editor/ItemEditor")]
    public static void OpenWindow()
    {
        var window = GetWindow<ItemWindow>();
        window.titleContent = new GUIContent("物品编辑器");
    }
}
