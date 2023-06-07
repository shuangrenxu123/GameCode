using System;
using System.Collections.Generic;

public class NetworkMessageRegister
{
    private static Dictionary<int, Type> types = new Dictionary<int, Type>();

    public static Type TryGetMessageType(int msgid)
    {
        Type type = null;
        types.TryGetValue(msgid, out type);
        return type;
    }

    public void Init()
    {
        RegisterMessageType(0, typeof(PlayerInfo.ping));
        RegisterMessageType(1, typeof(PlayerInfo.move));
        RegisterMessageType(2, typeof(PlayerInfo.Action));
    }
    /// <summary>
    /// 注册非热更的消息类型
    /// </summary>
    public static void RegisterMessageType(int msgID, Type classType)
    {
        // 判断是否重复
        if (types.ContainsKey(msgID))
            throw new Exception($"NetMessage {msgID} already exist.");

        types.Add(msgID, classType);
    }

    /// <summary>
    /// 获取指定消息ID的消息类型
    /// </summary>
    /// <param name="msgID">消息ID</param>
    /// <returns>如果找不到会报异常</returns>
    public static Type GetMessageType(int msgID)
    {
        Type type;
        if (types.TryGetValue(msgID, out type))
        {
            return type;
        }
        else
        {
            throw new KeyNotFoundException($"NetMessage {msgID} is not define.");
        }
    }
}
