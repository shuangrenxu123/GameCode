using ConsoleLog;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandUI : MonoBehaviour
{
    public TMP_InputField input;
    public TMP_Text Text;
    public GameObject parent;
    private void Start()
    {
        input.onSubmit.AddListener((string text) => Submit(text));
        ConsoleManager.Instance.OnOutput += OutputPanel;
    }

    private void OutputPanel(string arg1, string arg2)
    {
        if (arg1 != "" || arg1 != string.Empty ||arg1 != null)
        {
            var go = Instantiate(Text, parent.transform);
            go.text = arg1;
        }
    }

    private void Submit(string text)
    {
        ConsoleManager.Instance.SubmitCommand(text);
        input.text = string.Empty;
        
    }
    [Command("test")]
    private static void Test(int a)
    {
        Debug.Log(a);
    }
}
