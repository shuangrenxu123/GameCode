using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputSystemHandler : InputHandler
{
    [SerializeField]
    InputActionAsset InputActionsAsset = null;

    [SerializeField]
    bool filterByActionMap = false;

    [SerializeField]
    string gameplayActionMap = "Gameplay";

    [SerializeField]
    bool filterByControlScheme = false;

    [SerializeField]
    string controlSchemeName = "Keyboard Mouse";

    Dictionary<string, InputAction> inputActionsDictionary = new Dictionary<string, InputAction>();
    protected virtual void Awake()
    {

        if (InputActionsAsset == null)
        {
            Debug.Log("No input actions asset found!");
            return;
        }

        InputActionsAsset.Enable();

        if (filterByControlScheme)
        {
            string bindingGroup = InputActionsAsset.controlSchemes.First(x => x.name == controlSchemeName).bindingGroup;
            InputActionsAsset.bindingMask = InputBinding.MaskByGroup(bindingGroup);
        }

        ReadOnlyArray<InputAction> rawInputActions = new ReadOnlyArray<InputAction>();

        if (filterByActionMap)
        {
            rawInputActions = InputActionsAsset.FindActionMap(gameplayActionMap).actions;

            for (int i = 0; i < rawInputActions.Count; i++)
                inputActionsDictionary.Add(rawInputActions[i].name, rawInputActions[i]);

        }
        else
        {
            for (int i = 0; i < InputActionsAsset.actionMaps.Count; i++)
            {
                InputActionMap actionMap = InputActionsAsset.actionMaps[i];

                for (int j = 0; j < actionMap.actions.Count; j++)
                {
                    InputAction action = actionMap.actions[j];
                    inputActionsDictionary.Add(action.name, action);
                }

            }
        }
        for (int i = 0; i < rawInputActions.Count; i++)
        {
            inputActionsDictionary.Add(rawInputActions[i].name, rawInputActions[i]);
        }

    }

    public override bool GetBool(string actionName)
    {

        if (!inputActionsDictionary.TryGetValue(actionName, out InputAction inputAction))
            return false;

        return inputActionsDictionary[actionName].ReadValue<float>() >= InputSystem.settings.defaultButtonPressPoint;
    }

    public override float GetFloat(string actionName)
    {

        if (!inputActionsDictionary.TryGetValue(actionName, out InputAction inputAction))
            return 0f;

        return inputAction.ReadValue<float>();
    }

    public override Vector2 GetVector2(string actionName)
    {

        if (!inputActionsDictionary.TryGetValue(actionName, out InputAction inputAction))
            return Vector2.zero;

        return inputActionsDictionary[actionName].ReadValue<Vector2>();
    }
}

