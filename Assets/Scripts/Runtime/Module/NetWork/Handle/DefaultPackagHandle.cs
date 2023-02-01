using System;
using System.Collections.Generic;
using UnityEngine;

public class DefaultPackagHandle
{
    Dictionary<int, Action<object>> h = new();

    public void Init()
    {
        h.Add(1, Login);
    }

    public void HandleCB(DefaultNetWorkPackage package)
    {
        int id = package.MsgId;
        if (h.ContainsKey(id))
        {
            h[id].Invoke(package.Msgobj);
        }
        else
        {
            Debug.LogError("接收到了编号不存在的协议");
        }
    }
    void Login(object msg)
    {
        var tmp = msg as PlayerInfo.login;
        Debug.Log(tmp.Pw);
    }
}
