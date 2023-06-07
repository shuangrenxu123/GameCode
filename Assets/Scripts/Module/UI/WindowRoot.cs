using System.Collections.Generic;
using UnityEngine;

public abstract class WindowRoot : MonoBehaviour
{
    public Dictionary<string, UIEventListener> listeners = new Dictionary<string, UIEventListener>();
    public UIEventListener GetUIEvnetListener(string name)
    {
        if (!listeners.ContainsKey(name))
        {
            Transform childTransform = transform.FindChildByName(name);
            listeners.Add(name, UIEventListener.GetListener(childTransform));
        }
        return listeners[name];
    }

    public GameObject GetUIGameObject(string name)
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
