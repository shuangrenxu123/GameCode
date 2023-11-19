using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanInputType
{
    InputSystem,
    UIMobile,
    Custom
}
public abstract class InputHandler : MonoBehaviour
{
    public static InputHandler CreateInputHander(GameObject gameObject, HumanInputType inputType)
    {
        InputHandler inputHandler = null;
        switch (inputType)
        {
            case HumanInputType.InputSystem:
                inputHandler = gameObject.GetOrAddComponent<InputSystemHandler>();
                break;
            case HumanInputType.UIMobile:
                //to do UI InputHandle
                break;
        }
        return inputHandler;
    }

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
    InputHandler inputhandler = null;

    public InputHandler InputHandler { get { return inputhandler; } set { inputhandler = value; } }

    public void Initialize(GameObject gameObject)
    {
        if (inputhandler == null)
            InputHandler = InputHandler.CreateInputHander(gameObject, humanInputType);
    }


}
