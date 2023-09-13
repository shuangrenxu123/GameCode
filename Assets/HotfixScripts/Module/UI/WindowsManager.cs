using System.Collections.Generic;
using UnityEngine;

public class WindowsManager : ModuleSingleton<WindowsManager>, IModule
{
    Dictionary<string, WindowRoot> UIWindows;
    public void OnCreate(object createParam)
    {
        UIWindows = new Dictionary<string, WindowRoot>();
        WindowRoot[] wins = GameObject.FindObjectsOfType<WindowRoot>();
        foreach (WindowRoot w in wins)
            AddWindow(w);
    }
    public void OnUpdate()
    {

    }
    public T GetUiWindow<T>() where T : WindowRoot
    {
        string windowName = typeof(T).Name;
        if (UIWindows.ContainsKey(windowName))
            return UIWindows[windowName] as T;
        else
        {
            var w = GameObject.FindObjectOfType<T>();
            AddWindow(w);
            return w as T;
        }
    }
    private void AddWindow(WindowRoot window)
    {
        try
        {
            UIWindows.Add(window.GetType().Name, window);
        }
        catch (System.Exception)
        {
            Debug.Log(window.GetType().Name);
        }
    }
    public void EnableWindow<T>() where T : WindowRoot
    {
        var window = GetUiWindow<T>();
        window.gameObject.SetActive(true);
    }
    public void DisableWindow<T>() where T : WindowRoot
    {
        var window = GetUiWindow<T>();
        window.gameObject.SetActive(false);
    }
    public void DisableWindow(WindowRoot window)
    {
    }
}
