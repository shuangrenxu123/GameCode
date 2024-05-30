using ConsoleLog;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIWindow;
using UnityEngine;
using UnityEngine.UI;

public class CommandUI : UIWindowBase
{
    public TMP_InputField input;
    public Text Text;
    public GameObject parent;

    private Stack<Text> logStack;
    private const int MAXCOUNT = 20;
    private int logCount = 0;
    private void Start()
    {
        logStack = new Stack<Text>();
        input.onSubmit.AddListener((string text) => Submit(text));
        ConsoleManager.Instance.OnOutput += OutputPanel;
    }

    private void OutputPanel(string arg1, string arg2)
    {
        if (arg1 != "" && arg1 != string.Empty && arg1 != null)
        {
            var go = Instantiate(Text, parent.transform);
            if(ColorUtility.TryParseHtmlString(arg2,out var color))
            {
                go.color = color;
            }
            go.text = arg1;
            logStack.Push(go);
            logCount += 1;
        }
        if(logCount>= MAXCOUNT)
            Destroy(logStack.Pop());
    
    }

    private void Submit(string text)
    {
        ConsoleManager.Instance.SubmitCommand(text);
        input.text = string.Empty;
        
    }
}
