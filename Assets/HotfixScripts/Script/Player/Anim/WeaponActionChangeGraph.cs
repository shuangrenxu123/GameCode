using System;
using System.Collections.Generic;
using Character.Controller.State;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponActionChangeGraph", menuName = " AnimationConfig/WeaponActionChangeGraph", order = 1)]
public class WeaponActionChangeGraph : ScriptableObject
{
    [SerializeField]
    public string defaultActionName = "light_1";
    [SerializeField]
    WeaponAttackAnimationData
     allData;
    [SerializeField]
    public List<ActionGraph> actionGraphs = new();


    public string GetNextActionName(AttackKeyBoard keyBoard, string currentActionName, ECharacterMoveState moveState)
    {
        foreach (var actionGraph in actionGraphs)
        {
            if (actionGraph.actionNam == currentActionName)
            {
                foreach (var targetAction in actionGraph.targetActions)
                {
                    if (targetAction.KeyBoard == keyBoard && targetAction.moveState == moveState)
                    {
                        return targetAction.targetName;
                    }
                }
            }
        }

        // If no match found, return the default action name
        return defaultActionName;
    }
}

[System.Serializable]
public class ActionGraph
{
    public string actionNam;
    public List<TargetAction> targetActions;
}
[Serializable]
public class TargetAction
{
    public AttackKeyBoard KeyBoard;
    public string targetName;
    public ECharacterMoveState moveState;
}

public enum AttackKeyBoard
{
    Light,
    Heavy,
    Special,
}
