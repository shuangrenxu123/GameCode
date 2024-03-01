using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIWindow : MonoBehaviour
{
    public string WindowName
    {
        get => GetType().FullName;
    }
    private GraphicRaycaster raycaster;
    public Canvas canves { get; private set; }
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

    private void Awake()
    {
        canves = GetComponent<Canvas>();
        raycaster = GetComponent<GraphicRaycaster>();
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
    #region Method
    public abstract void OnCreate();
    public abstract void OnUpdate();
    public abstract void OnDelete();
    public abstract void OnFocus();
    public abstract void OnFocusOtherUI();

    #endregion
}
