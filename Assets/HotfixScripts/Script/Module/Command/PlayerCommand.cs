using System.Collections;
using System.Collections.Generic;
using System.Text;
using Character.Player;
using ConsoleLog;
using Helper;
using UI;
using UIWindow;
using UnityEngine;

public class PlayerCommand : MonoBehaviour
{
    [Variable("player")]
    static Player player;

    static readonly string SuccessColor = ColorUtility.ToHtmlStringRGB(new Color(0.38f, 0.94f, 0.61f));
    static readonly string WarningColor = ColorUtility.ToHtmlStringRGB(new Color(0.96f, 0.39f, 0.39f));

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        RegisterCommands();
    }

    static void RegisterCommands()
    {
        var console = ConsoleManager.Instance;
        console.RegisterCommand("Help", "Help()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "Help");
            Help();
            return null;
        });
        console.RegisterCommand("TestDialogue", "TestDialogue()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "TestDialogue");
            TestDialog();
            return null;
        });
        console.RegisterCommand("Test", "Test()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "Test");
            Test();
            return null;
        });
        console.RegisterCommand("Print", "Print(object obj)", args =>
        {
            ValidateArgumentCount(args, 1, 1, "Print");
            Print(args[0]);
            return null;
        });
        console.RegisterCommand("PlayerInfo", "PlayerInfo()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "PlayerInfo");
            ShowPlayerInfo();
            return null;
        });
        console.RegisterCommand("PlayerSetId", "PlayerSetId(string newId)", args =>
        {
            ValidateArgumentCount(args, 1, 1, "PlayerSetId");
            SetPlayerId(ReadArgument<string>(args, 0, "newId"));
            return null;
        });
        console.RegisterCommand("PlayerHeal", "PlayerHeal(int amount = 10)", args =>
        {
            ValidateArgumentCount(args, 0, 1, "PlayerHeal");
            HealPlayer(args.Count == 0 ? 10 : ReadArgument<int>(args, 0, "amount"));
            return null;
        });
        console.RegisterCommand("PlayerDamage", "PlayerDamage(int amount = 10)", args =>
        {
            ValidateArgumentCount(args, 0, 1, "PlayerDamage");
            DamagePlayer(args.Count == 0 ? 10 : ReadArgument<int>(args, 0, "amount"));
            return null;
        });
        console.RegisterCommand("PlayerTeleport", "PlayerTeleport(float x, float y, float z)", args =>
        {
            ValidateArgumentCount(args, 3, 3, "PlayerTeleport");
            TeleportPlayer(
                ReadArgument<float>(args, 0, "x"),
                ReadArgument<float>(args, 1, "y"),
                ReadArgument<float>(args, 2, "z"));
            return null;
        });
        console.RegisterCommand("ReadTable", "ReadTable()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "ReadTable");
            ReadTable();
            return null;
        });
        console.RegisterCommand("CreateLocalPackageData", "CreateLocalPackageData()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "CreateLocalPackageData");
            CreateLocalPackageData();
            return null;
        });
        console.RegisterCommand("ReadLocalPackageData", "ReadLocalPackageData()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "ReadLocalPackageData");
            ReadLocalPackageData();
            return null;
        });
        console.RegisterCommand("OpenPackagePanel", "OpenPackagePanel()", args =>
        {
            ValidateArgumentCount(args, 0, 0, "OpenPackagePanel");
            OpenPackagePanel();
            return null;
        });
        console.RegisterCommand("AddItem", "AddItem(int id = 1, int num = 1)", args =>
        {
            ValidateArgumentCount(args, 0, 2, "AddItem");
            int id = args.Count > 0 ? ReadArgument<int>(args, 0, "id") : 1;
            int num = args.Count > 1 ? ReadArgument<int>(args, 1, "num") : 1;
            AddItem(id, num);
            return null;
        });
    }

    static T ReadArgument<T>(List<object> args, int index, string name)
    {
        return (T)Helper.MethodCallable.ConvertValue(args[index], typeof(T), name);
    }

    static void ValidateArgumentCount(List<object> args, int minCount, int maxCount, string commandName)
    {
        int count = args?.Count ?? 0;
        if (count < minCount)
        {
            throw new Helper.RuntimeException($"{commandName} 缺少参数");
        }

        if (count > maxCount)
        {
            throw new Helper.RuntimeException($"{commandName} 参数过多");
        }
    }

    static void ReadTable()
    {
        PackageTable packageTable = Resources.Load<PackageTable>("UI/TableData/PackageTable");
        if (packageTable == null)
        {
            ConsoleManager.Instance.OutputToConsole("未找到背包配置表 UI/TableData/PackageTable", WarningColor);
            return;
        }

        foreach (PackageTableItem packageItem in packageTable.DataList)
        {
            ConsoleManager.Instance.OutputToConsole($"[id] {packageItem.id}, [name] {packageItem.name}");
        }
    }

    static void CreateLocalPackageData()
    {
        PackageLocalData.Instance.items = new List<PackageLocalItem>();
        PackageLocalData.Instance.SavePackage();
        ConsoleManager.Instance.OutputToConsole("已创建空的本地背包数据", SuccessColor);
    }

    static void ReadLocalPackageData()
    {
        List<PackageLocalItem> readItems = PackageLocalData.Instance.LoadPackage();
        if (readItems == null || readItems.Count == 0)
        {
            ConsoleManager.Instance.OutputToConsole("本地背包数据为空");
            return;
        }

        foreach (PackageLocalItem item in readItems)
        {
            ConsoleManager.Instance.OutputToConsole(item != null ? item.ToString() : "null");
        }
    }

    static void OpenPackagePanel()
    {
        AUIManager.Instance.OpenPanel(UIConst.PackagePanel);
    }

    static void AddItem(int id, int num)
    {
        if (GameManager.Instance == null)
        {
            ConsoleManager.Instance.OutputToConsole("场景中不存在 GameManager", WarningColor);
            return;
        }

        PackageLocalItem item = GameManager.Instance.AddItem(id, num);
        if (item != null)
        {
            ConsoleManager.Instance.OutputToConsole($"已添加物品 id={id}, num={num}", SuccessColor);
        }
    }
    [Command("Help", "打印所有已注册命令")]
    static void Help()
    {
        List<CommandSuggestion> commands = ConsoleManager.Instance.MatchCommandSuggestions(string.Empty);
        if (commands == null || commands.Count == 0)
        {
            ConsoleManager.Instance.OutputToConsole("当前没有已注册命令", WarningColor);
            return;
        }

        StringBuilder builder = new();
        builder.AppendLine("已注册命令:");
        for (int i = 0; i < commands.Count; i++)
        {
            builder.Append("  ");
            builder.Append(i + 1);
            builder.Append(". ");
            builder.Append(commands[i].DisplayText);
            if (i < commands.Count - 1)
            {
                builder.AppendLine();
            }
        }

        ConsoleManager.Instance.OutputToConsole(builder.ToString(), SuccessColor);
    }

    [Command("TestDialogue", "显示一条测试对话")]
    static void TestDialog()
    {
        DialogueCommon.ShowChat("测试用户", "对你说了一句悄悄话", Color.green);
    }

    [Command("Test", "输出测试命令文本")]
    static void Test()
    {
        ConsoleManager.Instance.OutputToConsole($"TestCommand");
    }

    [Command("Print", "输出传入参数")]
    static void Print(object obj)
    {
        ConsoleManager.Instance.OutputToConsole(obj.ToString());
    }

    [Command("PlayerInfo", "输出玩家当前状态")]
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

    [Command("PlayerSetId", "设置玩家 ID")]
    static void SetPlayerId(string newId)
    {
        if (!TryResolvePlayer(out var target))
            return;

        string oldId = target.id;
        target.id = newId;
        ConsoleManager.Instance.OutputToConsole($"player.id 从 {oldId} 修改为 {newId}", SuccessColor);
    }

    [Command("PlayerHeal", "恢复玩家生命值")]
    static void HealPlayer(int amount = 10)
    {
        if (!TryResolvePlayer(out var target) || !TryResolveHealth(target, out var hp))
            return;

        hp.Add(amount);
        ConsoleManager.Instance.OutputToConsole($"执行 player.CombatEntity.hp.Add({amount})，当前HP {hp.Value}/{hp.MaxValue}", SuccessColor);
    }

    [Command("PlayerDamage", "扣除玩家生命值")]
    static void DamagePlayer(int amount = 10)
    {
        if (!TryResolvePlayer(out var target) || !TryResolveHealth(target, out var hp))
            return;

        hp.Minus(amount);
        ConsoleManager.Instance.OutputToConsole($"执行 player.CombatEntity.hp.Minus({amount})，当前HP {hp.Value}/{hp.MaxValue}", SuccessColor);
    }

    [Command("PlayerTeleport", "传送玩家到指定坐标")]
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

    static bool TryResolveHealth(Player target, out Fight.Number.ResourceValue hp)
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
