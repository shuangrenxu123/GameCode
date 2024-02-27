using System;
using UnityEngine;

public enum HumanInputType
{
    InputSystem,
    UIMobile,
    Custom
}
public abstract class InputHandler : MonoBehaviour
{
    public abstract bool GetBool(string actionName);
    public abstract float GetFloat(string actionName);
    public abstract Vector2 GetVector2(string actionName);
}

[Serializable]
public class InputHandlerSettings
{
    [SerializeField]
    HumanInputType humanInputType = HumanInputType.InputSystem;
    [SerializeField]
    InputHandler inputhandler;
    public InputHandler InputHandler { get { return inputhandler; } set { inputhandler = value; } }

}
