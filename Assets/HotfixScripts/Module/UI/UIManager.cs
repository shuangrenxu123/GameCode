using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIWindow
{
    public class UIManager : ModuleSingleton<UIManager>, IModule
    {
        private const string UIRootName = "[UIRoot]";
        private const string UIInstanceRootName = "UI Form Instances";
        private const int GroupOrderStep = 1000;
        private const int WindowOrderStep = 10;

        private readonly Dictionary<string, UIWindowBase> loadedWindows = new(System.StringComparer.Ordinal);
        private readonly Dictionary<UIWindowGroup, UIGroupRuntime> uiGroups = new();
        private readonly List<UIGroupRuntime> cachedGroupSortList = new();

        private Transform uiRoot;
        private RectTransform uiInstanceRoot;

        public int UIWindowCount => loadedWindows.Count;
        public int UIGroupCount => uiGroups.Count;
        public Transform UIRoot => uiRoot;
        public RectTransform UIInstanceRoot => uiInstanceRoot;

        public void OnCreate(object createParam)
        {
            EnsureUIRoots();
            EnsureBuiltinGroups();
        }

        public void OnUpdate()
        {
            foreach (UIGroupRuntime group in uiGroups.Values)
            {
                group.Update();
            }
        }

        public bool HasUIGroup(UIWindowGroup group)
        {
            return uiGroups.ContainsKey(group);
        }

        public bool AddUIGroup(UIWindowGroup group)
        {
            return AddUIGroup(group, GetDefaultGroupDepth(group));
        }

        public bool AddUIGroup(UIWindowGroup group, int depth)
        {
            if (uiGroups.ContainsKey(group))
            {
                return false;
            }

            EnsureUIRoots();
            uiGroups.Add(group, new UIGroupRuntime(group, depth, GetOrCreateGroupRoot(group)));
            SortGroupRoots();
            return true;
        }

        public T OpenUI<T>(UIWindowBase prefab) where T : UIWindowBase
        {
            return OpenUI<T>(prefab, null);
        }

        public T OpenUI<T>(UIWindowBase prefab, object userData) where T : UIWindowBase
        {
            if (prefab == null)
            {
                Debug.LogError("Open UI failed, prefab is null.");
                return null;
            }

            string windowName = typeof(T).FullName;
            if (loadedWindows.TryGetValue(windowName, out UIWindowBase existingWindow))
            {
                RefocusWindow(existingWindow, userData);
                return existingWindow as T;
            }

            T instance = UnityEngine.Object.Instantiate(prefab) as T;
            if (instance == null)
            {
                Debug.LogError($"Open UI failed, prefab '{prefab.name}' can not instantiate as '{typeof(T).FullName}'.");
                return null;
            }

            EnsureGroup(instance.UIGroup);
            loadedWindows.Add(instance.WindowName, instance);

            UIGroupRuntime group = uiGroups[instance.UIGroup];
            instance.transform.SetParent(group.Root, false);
            instance.transform.SetAsLastSibling();
            group.Add(instance);

            instance.OnInit(userData);
            instance.OnOpen(userData);

            RefreshGroup(group);
            return instance;
        }

        public T GetUIWindow<T>() where T : UIWindowBase
        {
            string windowName = typeof(T).FullName;
            return loadedWindows.TryGetValue(windowName, out UIWindowBase window) ? window as T : null;
        }

        public UIWindowBase GetUIWindow(System.Type type)
        {
            if (type == null)
            {
                return null;
            }

            return loadedWindows.TryGetValue(type.FullName, out UIWindowBase window) ? window : null;
        }

        public void CloseUI<T>() where T : UIWindowBase => CloseUI(typeof(T), null);

        public void CloseUI<T>(object userData) where T : UIWindowBase => CloseUI(typeof(T), userData);

        public void CloseUI(System.Type type)
        {
            CloseUI(type, null);
        }

        public void CloseUI(System.Type type, object userData)
        {
            CloseUI(GetUIWindow(type), userData);
        }

        public void CloseUI(UIWindowBase window)
        {
            CloseUI(window, null);
        }

        public void CloseUI(UIWindowBase window, object userData)
        {
            if (window == null)
            {
                return;
            }

            if (!loadedWindows.Remove(window.WindowName))
            {
                return;
            }

            if (!uiGroups.TryGetValue(window.UIGroup, out UIGroupRuntime group))
            {
                window.OnClose(userData);
                GameObject.Destroy(window.gameObject);
                return;
            }

            group.Remove(window);
            window.OnClose(userData);
            RefreshGroup(group);
            GameObject.Destroy(window.gameObject);
        }

        public void CloseAll(object userData = null)
        {
            List<UIWindowBase> windows = new List<UIWindowBase>(loadedWindows.Values);
            foreach (UIWindowBase window in windows)
            {
                CloseUI(window, userData);
            }
        }

        public bool HasUI<T>() where T : UIWindowBase => loadedWindows.ContainsKey(typeof(T).FullName);

        public bool IsTopWindow(System.Type type)
        {
            if (type == null)
            {
                return false;
            }

            UIWindowBase topWindow = GetTopWindow();
            return topWindow != null && topWindow.WindowName == type.FullName;
        }

        public bool IsTopWindow<T>() where T : UIWindowBase => IsTopWindow(typeof(T));

        public UIWindowBase GetTopWindow()
        {
            UIWindowBase result = null;
            int topDepth = int.MinValue;

            foreach (UIGroupRuntime group in uiGroups.Values)
            {
                UIWindowBase topWindow = group.GetTopWindow();
                if (topWindow == null || group.Depth < topDepth)
                {
                    continue;
                }

                result = topWindow;
                topDepth = group.Depth;
            }

            return result;
        }

        public UIWindowBase GetTopWindow(UIWindowGroup group)
        {
            return uiGroups.TryGetValue(group, out UIGroupRuntime runtime) ? runtime.GetTopWindow() : null;
        }

        private void EnsureBuiltinGroups()
        {
            foreach (UIWindowGroup group in System.Enum.GetValues(typeof(UIWindowGroup)))
            {
                EnsureGroup(group);
            }
        }

        private void EnsureGroup(UIWindowGroup group)
        {
            if (!uiGroups.ContainsKey(group))
            {
                AddUIGroup(group);
            }
        }

        private int GetDefaultGroupDepth(UIWindowGroup group)
        {
            return ((int)group + 1) * GroupOrderStep;
        }

        private void EnsureUIRoots()
        {
            if (uiRoot == null)
            {
                GameObject rootObject = GameObject.Find(UIRootName);
                if (rootObject == null)
                {
                    rootObject = new GameObject(UIRootName);
                }

                Object.DontDestroyOnLoad(rootObject);
                uiRoot = rootObject.transform;
            }

            if (uiInstanceRoot == null)
            {
                Transform existingInstanceRoot = uiRoot.Find(UIInstanceRootName);
                GameObject instanceRootObject;
                if (existingInstanceRoot == null)
                {
                    instanceRootObject = new GameObject(UIInstanceRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    instanceRootObject.transform.SetParent(uiRoot, false);
                }
                else
                {
                    instanceRootObject = existingInstanceRoot.gameObject;
                    EnsureComponent<Canvas>(instanceRootObject);
                    EnsureComponent<CanvasScaler>(instanceRootObject);
                    EnsureComponent<GraphicRaycaster>(instanceRootObject);
                }

                uiInstanceRoot = instanceRootObject.GetComponent<RectTransform>();
                if (uiInstanceRoot == null)
                {
                    Object.Destroy(instanceRootObject);
                    instanceRootObject = new GameObject(UIInstanceRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    instanceRootObject.transform.SetParent(uiRoot, false);
                    uiInstanceRoot = instanceRootObject.GetComponent<RectTransform>();
                }

                ConfigureUIInstanceRoot(uiInstanceRoot);
            }
        }

        private void ConfigureUIInstanceRoot(RectTransform instanceRoot)
        {
            if (instanceRoot == null)
            {
                return;
            }

            instanceRoot.anchorMin = Vector2.zero;
            instanceRoot.anchorMax = Vector2.one;
            instanceRoot.pivot = new Vector2(0.5f, 0.5f);
            instanceRoot.offsetMin = Vector2.zero;
            instanceRoot.offsetMax = Vector2.zero;
            instanceRoot.localScale = Vector3.one;
            instanceRoot.localPosition = Vector3.zero;

            Canvas canvas = instanceRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = false;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = instanceRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            instanceRoot.GetComponent<GraphicRaycaster>();
        }

        private RectTransform GetOrCreateGroupRoot(UIWindowGroup group)
        {
            EnsureUIRoots();

            string groupRootName = GetGroupRootName(group);
            Transform groupRootTransform = uiInstanceRoot.Find(groupRootName);
            GameObject groupRootObject;
            if (groupRootTransform == null)
            {
                groupRootObject = new GameObject(groupRootName, typeof(RectTransform));
                groupRootObject.transform.SetParent(uiInstanceRoot, false);
            }
            else
            {
                groupRootObject = groupRootTransform.gameObject;
            }

            RectTransform groupRoot = groupRootObject.GetComponent<RectTransform>();
            if (groupRoot == null)
            {
                Object.Destroy(groupRootObject);
                groupRootObject = new GameObject(groupRootName, typeof(RectTransform));
                groupRootObject.transform.SetParent(uiInstanceRoot, false);
                groupRoot = groupRootObject.GetComponent<RectTransform>();
            }

            groupRoot.anchorMin = Vector2.zero;
            groupRoot.anchorMax = Vector2.one;
            groupRoot.pivot = new Vector2(0.5f, 0.5f);
            groupRoot.offsetMin = Vector2.zero;
            groupRoot.offsetMax = Vector2.zero;
            groupRoot.localScale = Vector3.one;
            groupRoot.localPosition = Vector3.zero;

            return groupRoot;
        }

        private void SortGroupRoots()
        {
            if (uiInstanceRoot == null || uiGroups.Count == 0)
            {
                return;
            }

            cachedGroupSortList.Clear();
            cachedGroupSortList.AddRange(uiGroups.Values);
            cachedGroupSortList.Sort((left, right) =>
            {
                int depthCompare = left.Depth.CompareTo(right.Depth);
                return depthCompare != 0 ? depthCompare : left.Group.CompareTo(right.Group);
            });

            for (int i = 0; i < cachedGroupSortList.Count; i++)
            {
                RectTransform root = cachedGroupSortList[i].Root;
                if (root != null)
                {
                    root.SetSiblingIndex(i);
                }
            }
        }

        private string GetGroupRootName(UIWindowGroup group)
        {
            return group.ToString();
        }

        private void RefocusWindow(UIWindowBase window, object userData)
        {
            if (window == null)
            {
                return;
            }

            if (!uiGroups.TryGetValue(window.UIGroup, out UIGroupRuntime group))
            {
                return;
            }

            group.Refocus(window);
            window.transform.SetParent(group.Root, false);
            window.transform.SetAsLastSibling();
            RefreshGroup(group);
            window.OnRefocus(userData);
        }

        private void RefreshGroup(UIGroupRuntime group)
        {
            if (group == null)
            {
                return;
            }

            bool pause = false;
            bool cover = false;
            int count = group.Windows.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                UIWindowBase window = group.Windows[i];
                if (window == null)
                {
                    continue;
                }

                int depthInGroup = i + 1;
                window.CanvasComponent.sortingOrder = group.Depth + i * WindowOrderStep + window.SortingOrderOffset;
                window.InternalSetDepth(depthInGroup);

                if (pause)
                {
                    if (!window.IsCoveredState)
                    {
                        window.InternalSetCoveredState(true);
                        window.OnCover();
                    }

                    if (!window.IsPausedState)
                    {
                        window.InternalSetPausedState(true);
                        window.OnPause();
                    }

                    continue;
                }

                if (window.IsPausedState)
                {
                    window.InternalSetPausedState(false);
                    window.OnResume();
                }

                if (cover)
                {
                    if (!window.IsCoveredState)
                    {
                        window.InternalSetCoveredState(true);
                        window.OnCover();
                    }
                }
                else
                {
                    if (window.IsCoveredState)
                    {
                        window.InternalSetCoveredState(false);
                        window.OnReveal();
                    }

                    cover = true;
                }

                if (window.PauseCoveredWindow)
                {
                    pause = true;
                }
            }
        }

        private sealed class UIGroupRuntime
        {
            public UIGroupRuntime(UIWindowGroup group, int depth, RectTransform root)
            {
                Group = group;
                Depth = depth;
                Root = root;
            }

            public UIWindowGroup Group { get; }
            public int Depth { get; }
            public RectTransform Root { get; }
            public List<UIWindowBase> Windows { get; } = new();

            public void Add(UIWindowBase window)
            {
                if (window != null && !Windows.Contains(window))
                {
                    Windows.Add(window);
                }
            }

            public void Remove(UIWindowBase window)
            {
                Windows.Remove(window);
            }

            public void Refocus(UIWindowBase window)
            {
                if (window == null)
                {
                    return;
                }

                if (Windows.Remove(window))
                {
                    Windows.Add(window);
                }
            }

            public UIWindowBase GetTopWindow()
            {
                return Windows.Count > 0 ? Windows[^1] : null;
            }

            public void Update()
            {
                for (int i = Windows.Count - 1; i >= 0; i--)
                {
                    UIWindowBase window = Windows[i];
                    if (window == null || !window.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    window.OnUpdate();
                    if (window.PauseCoveredWindow)
                    {
                        break;
                    }
                }
            }
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }
    }
}
