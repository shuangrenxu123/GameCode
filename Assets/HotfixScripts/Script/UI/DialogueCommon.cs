using ConsoleLog;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 用于给各处调用提示UI的中转接口
    /// </summary>
    public static class DialogueCommon
    {
        public static void ShowNormalTips(string content)
        {
            ConsoleManager.Instance.OutputToConsole(content);
        }

        public static void ShowChat(string name, string content, Color color)
        {
            ConsoleManager.Instance.OutputToConsole($"[{name}]: {content}"
                , ColorUtility.ToHtmlStringRGB(color));
        }

        public static void ShowGetItemTextTip(int itemID, int count = 1)
        {
            //To do
        }

        public static void ShowGetItemNoticeUI(int itemID, int count = 1)
        {
            //To do
        }
    }
}
