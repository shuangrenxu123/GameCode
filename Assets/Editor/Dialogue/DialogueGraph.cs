using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueEdtior
{


    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView graphView;
        private static string fileName = "New File";
        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("对话窗口");
        }

        private void OnEnable()
        {  
            ConstructGraphview();
            GenerateToolbar();
        }
        private void ConstructGraphview()
        {
            graphView = new DialogueGraphView()
            {
                name = "对话系统"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var textfield = new TextField("File name");
            textfield.SetValueWithoutNotify(fileName);
            textfield.MarkDirtyRepaint();
            textfield.RegisterValueChangedCallback(e => { fileName = e.newValue; });
            toolbar.Add(textfield);

            toolbar.Add(new Button(() => SaveData()) { text = "Save Data"});
            toolbar.Add(new Button(() => LoadData()) { text = "Load Data"});

            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            var i = GraphViewUtility.GetInstance(graphView);
            i.SaveData(fileName);
        }

        private void LoadData()
        {
            var i = GraphViewUtility.GetInstance(graphView);
            i.LoadData(fileName);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }
}
