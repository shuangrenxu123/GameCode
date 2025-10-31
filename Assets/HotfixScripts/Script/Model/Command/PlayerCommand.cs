using System.Collections;
using System.Collections.Generic;
using Character.Player;
using ConsoleLog;
using UI;
using UnityEngine;

public class PlayerCommand : MonoBehaviour
{
    static Player player;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
    }
    [Command("Help")]
    static void Help()
    {
        ConsoleManager.Instance.OutputToConsole("Help=========", ColorUtility.ToHtmlStringRGB(Color.green));
    }

    [Command("TestDialogue")]
    static void TestDialog()
    {
        DialogueCommon.ShowChat("测试用户", "对你说了一句悄悄话", Color.green);
    }
    [Command("Test")]
    static void Test()
    {
        ConsoleManager.Instance.OutputToConsole($"TestCommand");
    }
}
