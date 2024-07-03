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
    [SerializeField] Transform tipsParent;
    #region Message
    private Stack<Text> logStack;
    private const int MAXCOUNT = 20;
    private int logCount = 0;
    #endregion
    #region Command
    private List<string> commandStack;//历史命令
    private int currentCommandIndex = 0;
    private List<string> tipsCommand;
    #endregion

    Player player;
    private void Start()
    {
        logStack = new Stack<Text>();
        commandStack = new();
        tipsCommand = new();
        input.onSubmit.AddListener((string text) => SubmitCommand(text));
        input.onValueChanged.AddListener((string text)=>GetCommandTips(text));

        ConsoleManager.Instance.OnOutput += OutputPanel;


        input.ActivateInputField();
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
            go.text = arg1;
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
    private void SubmitCommand(string text)
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
            var tempText = $"[{player.id}] : {text}";
            ConsoleManager.Instance.OutputToConsole(tempText);
            SendPlayerMessage(tempText);
        }
        input.text = string.Empty;
        //UIManager.Instance.CloseUI<CommandUI>();
    }

    private void SendPlayerMessage(string mess)
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
        {
            NetWorkManager.Instance.SendMessage(player.id, 5, new PlayerInfo.PlayerMessage { Mes = mess });
        }

    }
    /// <summary>
    /// 获得命令提示
    /// </summary>
    /// <param name="inputText"></param>
    private void GetCommandTips(string inputText)
    {
        tipsParent.RemoveAllChildren();
        //todo 加上参数的提示
        if (inputText == "")
            return;
        bool isCommand = inputText[0] == '/';
        if (!isCommand)
            return;
        var names = ConsoleManager.Instance.CommandsNames;
        tipsCommand.Clear();
        var mainText = inputText.AsSpan().Slice(1, inputText.Length-1).ToString();
        foreach (var c in names)
        {
            if (c.Contains(mainText))
            {
                tipsCommand.Add(c);
            }
        }

        foreach (var i in tipsCommand)
        {
            var go = Instantiate(Text, tipsParent);
            go.text = i;
        }
    }
    /// <summary>
    /// 补全指令
    /// </summary>
    private void FillCommand()
    {
    }
    /// <summary>
    /// 切换补全指令
    /// </summary>
    private void SwitchFillCommand()
    {

    }

}
