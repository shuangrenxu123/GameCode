using System.Collections.Generic;
using UnityEngine;

namespace UIWindow
{
    public abstract class UIWindowBase : MonoBehaviour, IUIWindow
    {
        [SerializeField]
        private UIWindowGroup uiGroup = UIWindowGroup.Normal;

        [SerializeField]
        private bool pauseCoveredWindow = true;

        [SerializeField]
        private int sortingOrderOffset = 0;

        public Canvas CanvasComponent { get; private set; }
        public string WindowName => GetType().FullName;
        public CanvasGroup CanvasGroup { get; private set; }
        public virtual UIWindowGroup UIGroup => uiGroup;
        public virtual bool PauseCoveredWindow => pauseCoveredWindow;
        public int DepthInUIGroup { get; private set; }
        public int SortingOrderOffset => sortingOrderOffset;
        internal bool IsPausedState { get; private set; }
        internal bool IsCoveredState { get; private set; }

        public Dictionary<string, UIEventListener> listeners = new Dictionary<string, UIEventListener>();

        protected void Awake()
        {
            CanvasComponent = GetComponent<Canvas>();
            if (CanvasComponent == null)
            {
                CanvasComponent = gameObject.AddComponent<Canvas>();
            }

            CanvasComponent.overrideSorting = true;

            CanvasGroup = GetComponent<CanvasGroup>();
            if (CanvasGroup == null)
            {
                CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public UIEventListener GetUIEventListener(string name)
        {
            if (!listeners.TryGetValue(name, out UIEventListener listener))
            {
                Transform childTransform = transform.Find(name);
                listener = UIEventListener.GetListener(childTransform);
                listeners.Add(name, listener);
            }

            return listener;
        }

        public GameObject GetUIGameObject(string name)
        {
            Transform childTransform = transform.Find(name);
            return childTransform != null ? childTransform.gameObject : null;
        }

        public void RemoveUIEventListener(string name)
        {
            listeners.Remove(name);
        }

        public virtual void OnInit(object userData)
        {
        }

        public virtual void OnOpen(object userData)
        {
            SetVisible(true);
            SetInteractable(true);
        }

        public virtual void OnClose(object userData)
        {
        }

        public virtual void OnPause()
        {
            SetInteractable(false);
            SetVisible(false);
        }

        public virtual void OnResume()
        {
            SetVisible(true);
            SetInteractable(true);
        }

        public virtual void OnCover()
        {
            SetInteractable(false);
        }

        public virtual void OnReveal()
        {
            SetInteractable(true);
        }

        public virtual void OnRefocus(object userData)
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnDepthChanged(int depthInUIGroup)
        {
        }

        internal void InternalSetDepth(int depthInUIGroup)
        {
            DepthInUIGroup = depthInUIGroup;
            OnDepthChanged(depthInUIGroup);
        }

        internal void InternalSetPausedState(bool paused)
        {
            IsPausedState = paused;
        }

        internal void InternalSetCoveredState(bool covered)
        {
            IsCoveredState = covered;
        }

        protected void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        protected void SetInteractable(bool interactable)
        {
            if (CanvasGroup == null)
            {
                return;
            }

            CanvasGroup.interactable = interactable;
            CanvasGroup.blocksRaycasts = interactable;
        }
    }
}
