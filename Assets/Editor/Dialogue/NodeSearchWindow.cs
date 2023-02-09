using DialogueEdtior;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeSearchWindow : ScriptableObject,ISearchWindowProvider
{
    private DialogueGraphView view;
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
            }
        };
        return tree;

    }
    public void Init(DialogueGraphView dialogueGraphView)
    {
        this.view = dialogueGraphView;
    }
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        switch(SearchTreeEntry.userData)
        {
            case "dialogue":
                view.CreateNode("Node");
                
                return true;
            default:
                return true;
        }
    }
}
