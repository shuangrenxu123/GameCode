using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIWindow
{
    public abstract class UIWindowBase : MonoBehaviour, IUIWindow
    {
        protected CharacterBrain CharacterBrain;
        protected CharacterUIActions UIInput => CharacterBrain.CharacterUIActions;

        public Canvas canves { get; set; }
        public string WindowName { get => GetType().FullName; set { return; } }
        public CanvasGroup raycaster { get; set; }

        public Dictionary<string, UIEventListener> listeners = new Dictionary<string, UIEventListener>();

        #region event
        public event Action OnDeleteEvent;
        public event Action OnFocusEvent;
        public event Action OnFocusOtherUIEvent;
        #endregion
        private void Awake()
        {
            canves = GetComponent<Canvas>();
            raycaster = GetComponent<CanvasGroup>();
            CharacterBrain = FindFirstObjectByType<CharacterBrain>();
        }
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
        #region Method
        public virtual void OnCreate() { }
        public virtual void OnUpdate() { }
        public virtual void OnDelete()
        {
            OnDeleteEvent?.Invoke();
        }
        public virtual void OnFocus()
        {
            OnFocusEvent?.Invoke();
        }
        public virtual void OnFocusOtherUI()
        {
            OnFocusOtherUIEvent?.Invoke();
        }
        #endregion
    }
}