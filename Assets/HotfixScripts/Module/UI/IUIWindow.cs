using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IUIWindow 
{
    public string WindowName { get; set; }
    public CanvasGroup raycaster { get; set; }
    public Canvas canves { get; set; }
    public abstract void OnCreate();
    public abstract void OnUpdate();
    public abstract void OnDelete();
    public abstract void OnFocus();
    public abstract void OnFocusOtherUI();


}
