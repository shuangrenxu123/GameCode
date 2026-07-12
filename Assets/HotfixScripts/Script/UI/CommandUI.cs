using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using CharacterController;
using ConsoleLog;
using Helper;
using LitMotion;
using LitMotion.Extensions;
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
        [SerializeField, LabelText("最大日志数量"), Min(1)]
        int maxLogCount = 20;

        [SerializeField, LabelText("日志淡入淡出时间"), Min(0f)]
        float logFadeDuration = 0.2f;

        [SerializeField, LabelText("日志移动时间"), Min(0f)]
        float logMoveDuration = 0.2f;

        [SerializeField, LabelText("日志进入偏移"), Min(0f)]
        float logEnterOffsetY = 20f;

        [SerializeField, LabelText("空闲隐藏延迟"), Min(0f)]
        float idleHideDelay = 5f;

        [SerializeField, LabelText("整体淡出时间"), Min(0f)]
        float consoleFadeDuration = 0.3f;

        private readonly List<LogItem> logItems = new();
        RectTransform logParentRect;
        MotionHandle consoleFadeHandle;
        float lastLogTime;
        bool consoleHiddenByIdle;

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
            commandStack = new();
            tipsCommand = new();
            logParentRect = parent != null ? parent.transform as RectTransform : null;
            lastLogTime = Time.unscaledTime;
            input.onSubmit.AddListener((string text) => SubmitCommand(text));
            input.onValueChanged.AddListener((string text) => GetCommandTips(text));

            ConsoleManager.Instance.OnOutput += OutputPanel;

            player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();

        }
        public override void OnUpdate()
        {
            if (TryConsumeCancelInput())
            {
                HideInputPanel();
                return;
            }

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

            UpdateConsoleIdleFade();
        }

        private void OutputPanel(string arg1, string col)
        {
            if (string.IsNullOrEmpty(arg1))
            {
                return;
            }

            EnsureConsoleVisible();
            lastLogTime = Time.unscaledTime;

            Dictionary<LogItem, Vector2> oldPositions = CaptureLogPositions();
            LogItem newItem = CreateLogItem(arg1, col);
            logItems.Add(newItem);
            logCount = logItems.Count;

            LogItem removedItem = null;
            if (logItems.Count > maxLogCount)
            {
                removedItem = logItems[0];
                logItems.RemoveAt(0);
                logCount = logItems.Count;
                SetIgnoreLayout(removedItem, true);
            }

            RebuildLogLayout();
            AnimateLogPositions(oldPositions);
            AnimateLogEnter(newItem);

            if (removedItem != null)
            {
                AnimateLogExit(removedItem, oldPositions);
            }
        }

        private LogItem CreateLogItem(string message, string colorText)
        {
            var text = Instantiate(Text, parent.transform);
            text.text = message;
            ApplyLogColor(text, colorText);

            RectTransform rect = text.transform as RectTransform;
            CanvasGroup itemCanvasGroup = text.GetComponent<CanvasGroup>();
            if (itemCanvasGroup == null)
            {
                itemCanvasGroup = text.gameObject.AddComponent<CanvasGroup>();
            }

            itemCanvasGroup.alpha = 0f;
            return new LogItem(text, rect, itemCanvasGroup);
        }

        private void ApplyLogColor(TMP_Text text, string col)
        {
            string colorText = string.IsNullOrEmpty(col) ? "FFFFFF" : col.Trim();
            if (!colorText.StartsWith("#"))
            {
                colorText = "#" + colorText;
            }

            if (ColorUtility.TryParseHtmlString(colorText, out var color))
            {
                text.color = color;
            }
        }

        private Dictionary<LogItem, Vector2> CaptureLogPositions()
        {
            Dictionary<LogItem, Vector2> positions = new(logItems.Count);
            for (int i = 0; i < logItems.Count; i++)
            {
                LogItem item = logItems[i];
                if (item?.Rect == null)
                {
                    continue;
                }

                positions[item] = item.Rect.anchoredPosition;
            }

            return positions;
        }

        private void RebuildLogLayout()
        {
            if (logParentRect == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(logParentRect);
        }

        private void AnimateLogPositions(Dictionary<LogItem, Vector2> oldPositions)
        {
            for (int i = 0; i < logItems.Count; i++)
            {
                LogItem item = logItems[i];
                if (item?.Rect == null)
                {
                    continue;
                }

                Vector2 targetPosition = item.Rect.anchoredPosition;
                if (!oldPositions.TryGetValue(item, out Vector2 startPosition))
                {
                    continue;
                }

                item.Rect.anchoredPosition = startPosition;
                CancelHandle(ref item.MoveHandle);
                item.MoveHandle = LMotion.Create(startPosition, targetPosition, logMoveDuration)
                    .WithEase(Ease.OutCubic)
                    .BindToAnchoredPosition(item.Rect)
                    .AddTo(item.Text);
            }
        }

        private void AnimateLogEnter(LogItem item)
        {
            if (item?.Rect == null || item.CanvasGroup == null)
            {
                return;
            }

            Vector2 targetPosition = item.Rect.anchoredPosition;
            Vector2 startPosition = targetPosition - new Vector2(0f, logEnterOffsetY);
            item.Rect.anchoredPosition = startPosition;

            CancelHandle(ref item.MoveHandle);
            item.MoveHandle = LMotion.Create(startPosition, targetPosition, logMoveDuration)
                .WithEase(Ease.OutCubic)
                .BindToAnchoredPosition(item.Rect)
                .AddTo(item.Text);

            CancelHandle(ref item.FadeHandle);
            item.FadeHandle = LMotion.Create(0f, 1f, logFadeDuration)
                .WithEase(Ease.OutSine)
                .BindToAlpha(item.CanvasGroup)
                .AddTo(item.Text);
        }

        private void AnimateLogExit(LogItem item, Dictionary<LogItem, Vector2> oldPositions)
        {
            if (item?.Text == null || item.Rect == null || item.CanvasGroup == null)
            {
                return;
            }

            Vector2 startPosition = oldPositions != null && oldPositions.TryGetValue(item, out Vector2 oldPosition)
                ? oldPosition
                : item.Rect.anchoredPosition;
            Vector2 targetPosition = startPosition + GetExitMoveOffset(oldPositions);
            item.Rect.anchoredPosition = startPosition;

            CancelHandle(ref item.MoveHandle);
            item.MoveHandle = LMotion.Create(startPosition, targetPosition, logMoveDuration)
                .WithEase(Ease.OutCubic)
                .BindToAnchoredPosition(item.Rect)
                .AddTo(item.Text);

            CancelHandle(ref item.FadeHandle);
            item.FadeHandle = LMotion.Create(item.CanvasGroup.alpha, 0f, logFadeDuration)
                .WithEase(Ease.OutSine)
                .WithOnComplete(() =>
                {
                    if (item.Text != null)
                    {
                        Destroy(item.Text.gameObject);
                    }
                })
                .BindToAlpha(item.CanvasGroup)
                .AddTo(item.Text);
        }

        private Vector2 GetExitMoveOffset(Dictionary<LogItem, Vector2> oldPositions)
        {
            if (oldPositions != null)
            {
                for (int i = 0; i < logItems.Count; i++)
                {
                    LogItem item = logItems[i];
                    if (item?.Rect == null || !oldPositions.TryGetValue(item, out Vector2 oldPosition))
                    {
                        continue;
                    }

                    Vector2 offset = item.Rect.anchoredPosition - oldPosition;
                    if (offset.sqrMagnitude > 0.01f)
                    {
                        return offset;
                    }
                }
            }

            float fallbackDistance = 0f;
            if (Text != null && Text.transform is RectTransform textRect)
            {
                fallbackDistance = textRect.rect.height;
            }

            fallbackDistance += logEnterOffsetY;
            return Vector2.up * fallbackDistance;
        }

        private static void SetIgnoreLayout(LogItem item, bool ignoreLayout)
        {
            if (item?.Text == null)
            {
                return;
            }

            LayoutElement layoutElement = item.Text.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = item.Text.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = ignoreLayout;
        }

        private void EnsureConsoleVisible()
        {
            consoleHiddenByIdle = false;
            if (canvasGroup == null)
            {
                return;
            }

            CancelHandle(ref consoleFadeHandle);
            if (Mathf.Approximately(canvasGroup.alpha, 1f))
            {
                canvasGroup.alpha = 1f;
                return;
            }

            consoleFadeHandle = LMotion.Create(canvasGroup.alpha, 1f, consoleFadeDuration)
                .WithEase(Ease.OutSine)
                .BindToAlpha(canvasGroup)
                .AddTo(this);
        }

        private void UpdateConsoleIdleFade()
        {
            if (canvasGroup == null || consoleHiddenByIdle || IsInputEditing())
            {
                return;
            }

            if (Time.unscaledTime - lastLogTime < idleHideDelay)
            {
                return;
            }

            consoleHiddenByIdle = true;
            CancelHandle(ref consoleFadeHandle);
            consoleFadeHandle = LMotion.Create(canvasGroup.alpha, 0f, consoleFadeDuration)
                .WithEase(Ease.OutSine)
                .BindToAlpha(canvasGroup)
                .AddTo(this);
        }

        private bool IsInputEditing()
        {
            if (input == null)
            {
                return false;
            }

            return inputActive || input.isFocused || !string.IsNullOrEmpty(input.text);
        }

        private static void CancelHandle(ref MotionHandle handle)
        {
            if (handle.Equals(MotionHandle.None))
            {
                return;
            }

            handle.TryCancel();
            handle = MotionHandle.None;
        }

        private void SubmitCommand(string text)
        {
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
            EnsureConsoleVisible();
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
            ResolvePlayerBrain()?.DisableUIInput();

            if (!inputActive)
            {
                return;
            }

            inputActive = false;
            ClearTips();
            input.DeactivateInputField();
            input.gameObject.SetActive(false);
        }

        private bool TryConsumeCancelInput()
        {
            CharacterBrain brain = ResolvePlayerBrain();
            return brain != null
                && brain.TryGetInputCommand(CharacterInputType.UICancel, out var command)
                && command.BoolValue;
        }

        private CharacterBrain ResolvePlayerBrain()
        {
            if (player == null)
            {
                player = Player.Instance != null ? Player.Instance : FindFirstObjectByType<Player>();
            }

            return player != null ? player.brain : null;
        }

        private void OnDestroy()
        {
            ResolvePlayerBrain()?.DisableUIInput();

            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.OnOutput -= OutputPanel;
            }

            CancelHandle(ref consoleFadeHandle);
            for (int i = 0; i < logItems.Count; i++)
            {
                LogItem item = logItems[i];
                if (item == null)
                {
                    continue;
                }

                CancelHandle(ref item.MoveHandle);
                CancelHandle(ref item.FadeHandle);
            }
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

        sealed class LogItem
        {
            public readonly TMP_Text Text;
            public readonly RectTransform Rect;
            public readonly CanvasGroup CanvasGroup;
            public MotionHandle MoveHandle;
            public MotionHandle FadeHandle;

            public LogItem(TMP_Text text, RectTransform rect, CanvasGroup canvasGroup)
            {
                Text = text;
                Rect = rect;
                CanvasGroup = canvasGroup;
            }
        }

    }
}
