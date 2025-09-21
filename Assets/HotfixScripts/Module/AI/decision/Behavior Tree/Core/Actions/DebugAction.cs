using BT;
using UnityEngine;

public class DebugAction : BTAction<string, object>
{
    public enum DebugActionType
    {
        Debug,
        Warning,
        Error
    }

    string logInfo;

    DebugActionType type;
    public DebugAction(string info, DebugActionType type)
    {
        this.type = type;
        this.logInfo = info;
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
