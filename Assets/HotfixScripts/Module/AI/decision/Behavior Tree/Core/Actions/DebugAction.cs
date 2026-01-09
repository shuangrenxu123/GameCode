using BT;
using BT.EditorIntegration;
using UnityEngine;

[BTEditorNode("BTAction/DebugAction", BTEditorNodeKind.Action)]
public class DebugAction : BTAction
{
    public enum DebugActionType
    {
        Debug,
        Warning,
        Error
    }

    [BTEditorExpose]
    public string logInfo;

    [BTEditorExpose]
    public DebugActionType type = DebugActionType.Debug;

    public DebugAction()
    {

    }
    protected override BTResult Execute()
    {
        switch (type)
        {
            case DebugActionType.Debug:
                Debug.Log(logInfo);
                break;
            case DebugActionType.Warning:
                Debug.LogWarning(logInfo);
                break;
            case DebugActionType.Error:
                Debug.LogError(logInfo);
                break;
        }
        return BTResult.Success;
    }
}
