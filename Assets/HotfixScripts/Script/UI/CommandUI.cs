using ConsoleLog;
using Network;
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
    [SerializeField] Text Text;
    [SerializeField] GameObject parent;
    #region Message
    private Stack<Text> logStack;
    private const int MAXCOUNT = 20;
    private int logCount = 0;
    #endregion
    #region Command
    private List<string> commandStack;//¿˙ ∑√¸¡Ó
    private int currentCommandIndex = 0;
    #endregion

    Player player;
    private void Start()
    {
        logStack = new Stack<Text>();
        commandStack = new();
        input.onSubmit.AddListener((string text) => Submit(text));
        ConsoleManager.Instance.OnOutput += OutputPanel;

        player = FindFirstObjectByType<Player>();
        
    }
    public override void OnUpdate()
    {
        if (UIInput.cancel.Started)
        {
            CharacterBrain.DisableUIInput();
            UIManager.Instance.CloseUI(GetType());
        }
    }
    private void OutputPanel(string arg1, string col)
    {
        if (arg1 != "" && arg1 != string.Empty && arg1 != null)
        {
            var go = Instantiate(Text, parent.transform);
            if (ColorUtility.TryParseHtmlString(col, out var color))
            {
                go.color = color;
            }
            go.text = $"[{player.id}] : {arg1}";
            logStack.Push(go);
            logCount += 1;
        }
        if (logCount >= MAXCOUNT)
            Destroy(logStack.Pop());
    }
    public override void OnDelete()
    {
        ConsoleManager.Instance.OnOutput -= OutputPanel;
    }
    private void Submit(string text)
    {
        if (text == "" || text == string.Empty)
        {
            return;
        }
        var mianText = text.AsSpan();
        bool isCommand = text[0] == '/';
        if (isCommand)
        {
            ConsoleManager.Instance.SubmitCommand(mianText.Slice(1, text.Length - 1).ToString());
        }
        else
        {
            ConsoleManager.Instance.OutputToConsole(text);
        }
        input.text = string.Empty;
    }

    private void SendPlayerMessage(string mess)
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(player.id, 5, new PlayerInfo.PlayerMessage { Mes = mess });
        }

    }

}
