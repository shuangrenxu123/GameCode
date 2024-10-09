using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : ModuleSingleton<EventManager>, IModule
{
    private Dictionary<string, Action<object>> Listener = new Dictionary<string, Action<object>>();
    public void OnCreate(object createParam)
    {
    }

    public void OnUpdate()
    {
    }
    /// <summary>
    /// 监听一个事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="listener"></param>
    public void AddListener(string name, Action<object> listener)
    {
        if (Listener.ContainsKey(name))
            Listener[name] += listener;
        else
            Listener.Add(name, listener);
    }
    /// <summary>
    /// 移除一个事件的监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="listener"></param>
    public void RemoveListener(string name, Action<object> listener)
    {
        if (Listener.ContainsKey(name))
            Listener[name] -= listener;
        else
            Debug.LogError("事件不存在");
    }
    /// <summary>
    /// 触发一个事件。后面为他的参数列表
    /// </summary>
    /// <param name="name"></param>
    /// <param name="mess"></param>
    public void SendMessage(string name, object mess)
    {
        if (Listener.ContainsKey(name))
        {
            Listener[name].Invoke(mess);
        }
    }
    public void ClearListener(string name)
    {
        Listener.Remove(name);
    }
    public int GetAllListener(string name)
    {
        return Listener[name].GetInvocationList().Length;
    }
}
