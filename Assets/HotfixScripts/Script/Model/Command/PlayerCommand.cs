using System.Collections;
using System.Collections.Generic;
using ConsoleLog;
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

    [Command("Test")]
    static void Test()
    {
        ConsoleManager.Instance.OutputToConsole($"TestCommand");
    }
}
