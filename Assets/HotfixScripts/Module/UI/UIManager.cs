using System;
using System.Collections.Generic;

using UnityEngine;
namespace UIWindow
{
    public class UIManager : ModuleSingleton<UIManager>, IModule
    {
        private readonly List<IUIWindow> currentUIStack = new(20);
        public int UIWindowCount => currentUIStack.Count;
        public void OnCreate(object createParam)
        {

        }
        public void OnUpdate()
        {
            for (int i = 0; i < currentUIStack.Count; i++)
            {
                currentUIStack[i].OnUpdate();
            }
        }
        public T OpenUI<T>(UIWindowBase prefab) where T : UIWindowBase
        {
            string name = prefab.WindowName;
            var window = GetWindow(name);
            if (window != null)
            {
                var topWindow = GetTopWindow();
                if (topWindow != window)
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
            return window as T;
        }
        public T GetUIWindow<T>() where T : UIWindowBase
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
        public void CloseUI<T>() where T : UIWindowBase => CloseUI(typeof(T));
        public void CloseUI(Type t)
        {
            string name = t.FullName;
            var window = GetWindow(name);
            if (window != null)
            {
                window.OnDelete();
                Pop(window);

                GameObject.Destroy(window.gameObject);
            }
        }
        public bool HasUI<T>() where T : UIWindowBase => IsContains(typeof(T).FullName);
        public bool IsTopWindow(Type t)
        {
            var top = GetTopWindow();
            if (top.WindowName == t.FullName)
            {
                return true;
            }
            return false;

        }
        public bool IsTopWindow<T>() where T : UIWindowBase => IsTopWindow(typeof(T));

        public UIWindowBase GetTopWindow()
        {
            if (currentUIStack.Count != 0)
                return currentUIStack[^1] as UIWindowBase;
            else
            {
                return null;
            }
        }
        //==========private============================

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
        private UIWindowBase GetWindow(string name)
        {
            foreach (var ui in currentUIStack)
            {
                if (ui.WindowName == name)
                {
                    return ui as UIWindowBase;
                }
            }

            return null;
        }
        private void Push(UIWindowBase window)
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
        private void Pop(UIWindowBase window)
        {
            currentUIStack.Remove(window);
        }





    }
}