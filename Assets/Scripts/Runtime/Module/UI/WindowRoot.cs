using System.Collections.Generic;
using UnityEngine;

public abstract class WindowRoot : MonoBehaviour
{
    public Dictionary<string, UIEventListener> listeners = new Dictionary<string, UIEventListener>();
    /// <summary>
    /// 获得canves界面下面的子类ui
    /// </summary>
    /// <param name="name">子类ui的名字</param>
    /// <returns></returns>
    public UIEventListener GetUIEvnetListener(string name)
    {
        if (!listeners.ContainsKey(name))
        {
            Transform childTransform = transform.FindChildByName(name);
            listeners.Add(name, UIEventListener.GetListener(childTransform));
        }
        return listeners[name];
    }

    public abstract void UpdateWindow();
    public GameObject GetUI(string name)
    {
        return transform.FindChildByName(name).gameObject;
    }
    public void RemoveUIEventListener(string name)
    {
        if (listeners.ContainsKey(name))
        {
            listeners.Remove(name);
        }
    }

    public abstract void Start();

    public abstract void Update();
}
