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
        string window = typeof(T).Name;
        if (UIWindows.ContainsKey(window))
            return UIWindows[window] as T;
        else
            return null;
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
        //window.gameObject.SetActive(false);
    }
    public void EnableWindow<T>() where T : WindowRoot
    {
        var window = GetUiWindow<T>();
        window.gameObject.SetActive(true);
    }
    public void DisableWindow<T>() where T: WindowRoot
    {
        var window = GetUiWindow<T>();
        window.gameObject.SetActive(false);
    }
}
