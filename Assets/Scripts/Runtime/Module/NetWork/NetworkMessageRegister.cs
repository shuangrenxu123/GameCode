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
        RegisterMessageType(1, typeof(PlayerInfo.login));
    }
    /// <summary>
    /// ע����ȸ�����Ϣ����
    /// </summary>
    public static void RegisterMessageType(int msgID, Type classType)
    {
        // �ж��Ƿ��ظ�
        if (types.ContainsKey(msgID))
            throw new Exception($"NetMessage {msgID} already exist.");

        types.Add(msgID, classType);
    }

    /// <summary>
    /// ��ȡָ����ϢID����Ϣ����
    /// </summary>
    /// <param name="msgID">��ϢID</param>
    /// <returns>����Ҳ����ᱨ�쳣</returns>
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
