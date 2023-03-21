using DialogueEdtior;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogueGraphView view;
    private EditorWindow editorWindow;
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"),0),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"),1),
            new SearchTreeEntry(new GUIContent("base node"))
            {
                userData = "dialogue",
                level =2,
            },

            new SearchTreeEntry(new GUIContent("Group"))
            {
                userData = "group",
                level = 1,
            }
        };
        return tree;

    }
    public void Init(DialogueGraphView dialogueGraphView, EditorWindow window)
    {
        this.view = dialogueGraphView;
        this.editorWindow = window;
    }
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var position = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);

        var loaclposition = view.contentContainer.WorldToLocal(position);
        switch (SearchTreeEntry.userData)
        {
            case "dialogue":
                view.CreateNode(loaclposition, "Node");
                return true;
            case "group":
                view.Creategroup(view.selection.ToList().Cast<GraphElement>().ToList());
                return true;
            default:
                return false;
        }
    }
}
