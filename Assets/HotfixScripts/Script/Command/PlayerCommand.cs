using ConsoleLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommand : MonoBehaviour
{
    static Player player;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
    }
    [Command("Test")]
    static void Test()
    {
        ConsoleManager.Instance.OutputToConsole($"TestCommand");
    }
    [Command("PlayerInfo")]
    static void PrintPlayerInfo()
    {

    }
    [Command("giveItem")]
    static void AddItem(int id, int num = 1)
    {
        Debug.Log("add " + id);
        ConsoleManager.Instance.OutputToConsole($"�����Ʒ{num}��");
    }
    [Command("AddBuff")]
    static void AddBuff(string name)
    {
        player.CombatEntity.buffManager.AddBuff(name);
    }
}
