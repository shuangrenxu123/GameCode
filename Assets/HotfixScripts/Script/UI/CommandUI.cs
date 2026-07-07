using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using ConsoleLog;
using Helper;
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
        [SerializeField, LabelText("命令输入框")]
        TMP_InputField input;

        [SerializeField, LabelText("文本模板")]
        TMP_Text Text;

        [SerializeField, LabelText("日志父节点")]
        GameObject parent;

        [SerializeField, LabelText("提示父节点")]
        Transform tipsParent;

        [SerializeField, LabelText("透明度")]
        CanvasGroup canvasGroup;

        [SerializeField, LabelText("提示普通颜色")]
        Color tipNormalColor = Color.white;

        [SerializeField, LabelText("提示高亮颜色")]
        Color tipHighlightColor = Color.yellow;

        #region Message
        private Queue<TMP_Text> logQueue;
        private const int MAXCOUNT = 20;

        private int logCount = 0;
        #endregion

        #region Command
        private List<string> commandStack;//
        private int currentCommandIndex = 0;
        private List<CommandSuggestion> tipsCommand;
        private readonly List<TMP_Text> activeTipItems = new();
        private const int MaxSuggestionCount = 6;
        private int selectedTipIndex = -1;
        private bool suppressTipRefresh;
        #endregion

        Player player;
        bool inputActive;
        private void Start()
        {
            logQueue = new Queue<TMP_Text>();
            commandStack = new();
            tipsCommand = new();
            input.onSubmit.AddListener((string text) => SubmitCommand(text));
            input.onValueChanged.AddListener((string text) => GetCommandTips(text));

            ConsoleManager.Instance.OnOutput += OutputPanel;

            player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();

        }
        public override void OnUpdate()
        {
            // if (UIInput.cancel.Started)
            // {
            //     HideInputPanel();
            //     UIManager.Instance.CloseUI(GetType());
            // }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FillCommand();
            }

            if (tipsCommand != null && tipsCommand.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Keypad8))
                {
                    MoveTipSelection(-1);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    MoveTipSelection(1);
                }
            }

            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && IsSelectingSuggestion())
            {
                ApplySelectedSuggestion();
            }
        }

        private void OutputPanel(string arg1, string col)
        {
            if (!string.IsNullOrEmpty(arg1))
            {
                var go = Instantiate(Text, parent.transform);
                string colorText = string.IsNullOrEmpty(col) ? "FFFFFF" : col.Trim();
                if (!colorText.StartsWith("#"))
                {
                    colorText = "#" + colorText;
                }

                if (ColorUtility.TryParseHtmlString(colorText, out var color))
                {
                    go.color = color;
                }
                go.text = arg1;
                logQueue.Enqueue(go);
                logCount += 1;
            }
            if (logCount > MAXCOUNT)
            {
                TMP_Text oldLog = logQueue.Dequeue();
                if (oldLog != null)
                {
                    Destroy(oldLog.gameObject);
                }

                logCount -= 1;
            }
        }


        private void SubmitCommand(string text)
        {
            if (IsSelectingSuggestion())
            {
                ApplySelectedSuggestion();
                return;
            }

            if (text == "" || text == string.Empty)
            {
                HideInputPanel();
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
            ClearTips();
            HideInputPanel();
        }

        private void SendPlayerMessage(string mess)
        {
            // if (NetWorkManager.Instance.Client.state == ENetWorkState.Connected)
            // {
            //     NetWorkManager.Instance.Client.SendMessage(player.id, 5, new PlayerInfo.PlayerMessage { Mes = mess });
            // }

        }
        private void GetCommandTips(string inputText)
        {
            if (suppressTipRefresh)
            {
                suppressTipRefresh = false;
                return;
            }

            ClearTips();

            if (string.IsNullOrEmpty(inputText) || inputText[0] != '/')
            {
                return;
            }

            if (inputText.Length <= 1)
            {
                return;
            }

            string keyword = inputText.Length > 1 ? inputText.Substring(1) : string.Empty;
            var matches = ConsoleManager.Instance.MatchCommandSuggestions(keyword);
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            int count = Mathf.Min(matches.Count, MaxSuggestionCount);
            tipsCommand.Clear();
            for (int i = 0; i < count; i++)
            {
                tipsCommand.Add(matches[i]);
                var go = Instantiate(Text, tipsParent);
                go.text = matches[i].DisplayText;
                go.color = tipNormalColor;
                activeTipItems.Add(go);
            }

            selectedTipIndex = count > 0 ? 0 : -1;
            UpdateTipHighlight();
        }

        private void FillCommand()
        {
            if (tipsCommand == null || tipsCommand.Count == 0)
            {
                return;
            }

            if (!IsSelectingSuggestion())
            {
                selectedTipIndex = 0;
            }

            ApplySelectedSuggestion();
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
            selectedTipIndex = -1;
        }

        public void ShowInputPanel()
        {
            if (inputActive)
            {
                input.ActivateInputField();
                return;
            }

            inputActive = true;
            input.gameObject.SetActive(true);
            input.text = string.Empty;
            input.ActivateInputField();
        }

        public void HideInputPanel()
        {
            if (!inputActive)
            {
                return;
            }

            inputActive = false;
            input.DeactivateInputField();
            input.gameObject.SetActive(false);
        }

        private void MoveTipSelection(int direction)
        {
            if (tipsCommand == null || tipsCommand.Count == 0)
            {
                return;
            }

            int maxIndex = Mathf.Min(tipsCommand.Count, activeTipItems.Count) - 1;
            selectedTipIndex = Mathf.Clamp(selectedTipIndex + direction, 0, maxIndex);
            UpdateTipHighlight();
        }

        private void UpdateTipHighlight()
        {
            for (int i = 0; i < activeTipItems.Count; i++)
            {
                if (activeTipItems[i] == null)
                {
                    continue;
                }

                activeTipItems[i].color = i == selectedTipIndex ? tipHighlightColor : tipNormalColor;
            }
        }

        private bool IsSelectingSuggestion()
        {
            return tipsCommand != null
                && tipsCommand.Count > 0
                && selectedTipIndex >= 0
                && selectedTipIndex < tipsCommand.Count
                && selectedTipIndex < activeTipItems.Count;
        }

        private void ApplySelectedSuggestion()
        {
            if (!IsSelectingSuggestion())
            {
                return;
            }

            suppressTipRefresh = true;
            var suggestion = tipsCommand[selectedTipIndex];
            ClearTips();
            input.text = $"/{suggestion.InsertText}";
            input.MoveTextEnd(false);
        }

    }
}
