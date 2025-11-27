using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using ConsoleLog;
using Network;
using Sirenix.OdinInspector;
using TMPro;
using UIWindow;
using UnityEngine;
using UnityEngine.UI;
namespace UIPanel.Console
{
    public class CommandUI : UIWindowBase
    {
        [SerializeField]
        TMP_InputField input;

        [SerializeField]
        TMP_Text Text;

        [SerializeField]
        GameObject parent;

        [SerializeField]
        Transform tipsParent;

        [SerializeField, LabelText("透明度")]
        CanvasGroup canvasGroup;

        #region Message
        private Stack<TMP_Text> logStack;
        private const int MAXCOUNT = 20;

        private int logCount = 0;
        #endregion

        #region Command
        private List<string> commandStack;//
        private int currentCommandIndex = 0;
        private List<string> tipsCommand;
        private readonly List<TMP_Text> activeTipItems = new();
        private const int MaxSuggestionCount = 6;
        #endregion

        Player player;
        private void Start()
        {
            logStack = new Stack<TMP_Text>();
            commandStack = new();
            tipsCommand = new();
            input.onSubmit.AddListener((string text) => SubmitCommand(text));
            input.onValueChanged.AddListener((string text) => GetCommandTips(text));

            ConsoleManager.Instance.OnOutput += OutputPanel;

            input.gameObject.SetActive(true);
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

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FillCommand();
            }
        }

        private void OutputPanel(string arg1, string col)
        {
            if (arg1 != "" && arg1 != string.Empty && arg1 != null)
            {
                var go = Instantiate(Text, parent.transform);
                if (ColorUtility.TryParseHtmlString($"#{col}", out var color))
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
            var mainText = text.AsSpan();
            bool isCommand = text[0] == '/';
            if (isCommand)
            {
                ConsoleManager.Instance.SubmitCommand(mainText.Slice(1, text.Length - 1).ToString());
            }
            else
            {
                var tempText = $"[{player.id}] : {text}";
                ConsoleManager.Instance.OutputToConsole(tempText);
                SendPlayerMessage(tempText);
            }
            input.text = string.Empty;
            input.gameObject.SetActive(false);
            ClearTips();
            Player.Instance.brain.DisableUIInput();
        }

        private void SendPlayerMessage(string mess)
        {
            if (NetWorkManager.Instance.state == ENetWorkState.Connected)
            {
                NetWorkManager.Instance.SendMessage(player.id, 5, new PlayerInfo.PlayerMessage { Mes = mess });
            }

        }
        private void GetCommandTips(string inputText)
        {
            ClearTips();

            if (string.IsNullOrEmpty(inputText) || inputText[0] != '/')
            {
                return;
            }

            string keyword = inputText.Length > 1 ? inputText.Substring(1) : string.Empty;
            var matches = ConsoleManager.Instance.MatchCommands(keyword);
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            tipsCommand.Clear();
            tipsCommand.AddRange(matches);

            int count = Mathf.Min(matches.Count, MaxSuggestionCount);
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(Text, tipsParent);
                go.text = matches[i];
                activeTipItems.Add(go);
            }
        }

        private void FillCommand()
        {
            if (tipsCommand == null || tipsCommand.Count == 0)
                return;

            input.text = $"/{tipsCommand[0]}";
            input.MoveTextEnd(false);
            ClearTips();
        }

        private void SwitchFillCommand()
        {

        }

        private void ClearTips()
        {
            if (activeTipItems.Count > 0)
            {
                for (int i = 0; i < activeTipItems.Count; i++)
                {
                    if (activeTipItems[i] != null)
                    {
                        Destroy(activeTipItems[i].gameObject);
                    }
                }
                activeTipItems.Clear();
            }

            tipsCommand?.Clear();
        }

    }
}