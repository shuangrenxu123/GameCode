using System.Collections;
using System.Collections.Generic;
using Character.Player;
using ConsoleLog;
using UI;
using UnityEngine;

public class PlayerCommand : MonoBehaviour
{
    [ConsoleVariable("player")]
    static Player player;

    static readonly string SuccessColor = ColorUtility.ToHtmlStringRGB(new Color(0.38f, 0.94f, 0.61f));
    static readonly string WarningColor = ColorUtility.ToHtmlStringRGB(new Color(0.96f, 0.39f, 0.39f));

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
    [Command("Print")]
    static void Print(object obj)
    {
        ConsoleManager.Instance.OutputToConsole(obj.ToString());
    }

    [Command("Player.Info")]
    static void ShowPlayerInfo()
    {
        if (!TryResolvePlayer(out var target))
            return;

        var hp = target.CombatEntity?.hp;
        Vector3 position = target.transform.position;
        string info = hp == null
            ? $"Player[{target.id}] 当前位置:{position}"
            : $"Player[{target.id}] HP:{hp.Value}/{hp.MaxValue} 位置:{position}";

        ConsoleManager.Instance.OutputToConsole(info, SuccessColor);
    }

    [Command("Player.SetId")]
    static void SetPlayerId(string newId)
    {
        if (!TryResolvePlayer(out var target))
            return;

        string oldId = target.id;
        target.id = newId;
        ConsoleManager.Instance.OutputToConsole($"player.id 从 {oldId} 修改为 {newId}", SuccessColor);
    }

    [Command("Player.Heal")]
    static void HealPlayer(int amount = 10)
    {
        if (!TryResolvePlayer(out var target) || !TryResolveHealth(target, out var hp))
            return;

        hp.Add(amount);
        ConsoleManager.Instance.OutputToConsole($"执行 player.CombatEntity.hp.Add({amount})，当前HP {hp.Value}/{hp.MaxValue}", SuccessColor);
    }

    [Command("Player.Damage")]
    static void DamagePlayer(int amount = 10)
    {
        if (!TryResolvePlayer(out var target) || !TryResolveHealth(target, out var hp))
            return;

        hp.Minus(amount);
        ConsoleManager.Instance.OutputToConsole($"执行 player.CombatEntity.hp.Minus({amount})，当前HP {hp.Value}/{hp.MaxValue}", SuccessColor);
    }

    [Command("Player.Teleport")]
    static void TeleportPlayer(float x, float y, float z)
    {
        if (!TryResolvePlayer(out var target))
            return;

        target.transform.position = new Vector3(x, y, z);
        ConsoleManager.Instance.OutputToConsole($"player.transform.position 已设置为 ({x:F1}, {y:F1}, {z:F1})", SuccessColor);
    }

    static bool TryResolvePlayer(out Player target)
    {
        target = player != null ? player : Player.Instance;
        if (target == null)
        {
            ConsoleManager.Instance.OutputToConsole("未找到 player 实例，确保场景中存在 Player", WarningColor);
            return false;
        }

        player = target;
        return true;
    }

    static bool TryResolveHealth(Player target, out Fight.Number.HealthPoint hp)
    {
        hp = target.CombatEntity?.hp;
        if (hp == null)
        {
            ConsoleManager.Instance.OutputToConsole("player.CombatEntity.hp 未初始化", WarningColor);
            return false;
        }

        return true;
    }
}
