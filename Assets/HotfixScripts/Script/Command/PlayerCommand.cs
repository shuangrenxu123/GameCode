using ConsoleLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommand
{
    static Player player;
    [Command("PlayerInfo")]
    static void PrintPlayerInfo()
    {

    }
    [Command("giveItem")]
    static void AddItem(int id,int num = 1)
    {
        ConsoleManager.Instance.OutputToConsole($"获得物品{num}个");
    }
}
