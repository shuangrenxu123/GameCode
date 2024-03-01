using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIManager : ModuleSingleton<UIManager>, IModule
{
    private readonly List<UIWindow> currentUIStack = new (20);

    public void OnCreate(object createParam)
    {

    }
    public void OnUpdate()
    {
        foreach (var window in currentUIStack)
        {
            window.OnUpdate();
        }
    }

    public void OpenUI<T>(string localname) where T :UIWindow
    {
        var prefab = Resources.Load<UIWindow>(localname);
        string name = typeof(T).FullName;
        var window = GetWindow(name);
        if (window != null)
        {
            var topwindow = GetTopWindow();
            if (topwindow != window)
            {
                Pop(window);
                window.OnFocus();
                Push(window);
            }
        }
        else
        {
            window = UnityEngine.Object.Instantiate(prefab);
            Push(window);
            window.OnCreate();
            
        }

        foreach (var ui in currentUIStack)
        {
            if (ui.WindowName == window.WindowName)
            {
                continue;
            }
            ui.OnFocusOtherUI();

        }
    }
    public void OpenUI(UIWindow prefab)
    {
        string name = prefab.WindowName;
        var window = GetWindow(name);
        if (window != null)
        {
            var topwindow = GetTopWindow();
            if (topwindow != window)
            {
                Pop(window);
                window.OnFocus();
                Push(window);
            }
        }
        else
        {
            window = UnityEngine.Object.Instantiate(prefab);
            Push(window);
            window.OnCreate();

        }

        foreach (var ui in currentUIStack)
        {
            if (ui.WindowName == window.WindowName)
            {
                continue;
            }
            ui.OnFocusOtherUI();

        }
    }
    public T GetUIWindow<T>() where T :UIWindow
    {
        foreach (var ui in currentUIStack)
        {
            if (ui.WindowName == typeof(T).FullName)
            {
                return ui as T;
            }
        }

        return null;
    }
    public void CloseUI<T>() where T : UIWindow
    {
        string name = typeof(T).FullName;
        var window = GetWindow(name);
        if (window != null)
        {
            Pop(window);
            window.OnDelete();
            GameObject.Destroy(window.gameObject);
        }
    }
    public void HasUI()
    {
        
    }
//==========private============================

    private UIWindow GetTopWindow()
    {
        if(currentUIStack.Count !=0)
            return currentUIStack[^1];
        else
        {
            return null;
        }
    }
    private bool IsContains(string name)
    {
        foreach (var ui in currentUIStack)
        {
            if (ui.WindowName == name)
            {
                return true;
            }
        }

        return false;
    }
    private UIWindow GetWindow(string name)
    {
        foreach (var ui in currentUIStack)
        {
            if (ui.WindowName == name)
            {
                return ui;
            }
        }

        return null;
    }
    private void Push(UIWindow window)
    {
        if (IsContains(window.WindowName))
        {
            return;
        }

        if (currentUIStack.Count == 0)
        {
            window.canves.sortingOrder = 0;

        }
        else
        {
            window.canves.sortingOrder = currentUIStack[^1].canves.sortingOrder + 10;
        }
        currentUIStack.Add(window);
    }   
    private void Pop(UIWindow window)
    {
        currentUIStack.Remove(window);
    }

}
