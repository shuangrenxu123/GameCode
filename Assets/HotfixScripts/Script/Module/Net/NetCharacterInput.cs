using System;
using System.Collections.Generic;
using Network.Tcp;
using PlayerInfo;
using UnityEngine;
using ActionMessage = PlayerInfo.Action;

public class NetCharacterInput : MonoBehaviour
{
    [SerializeField]
    private StateSyncMgr stateSyncMgr;

    [SerializeField, Min(0f)]
    private float actionPulseDuration = 0.1f;

    private TcpClient client;
    private bool registered;

    private CharacterActions characterActions = new CharacterActions();
    private bool actionsInitialized;

    private Vector3 netPosition = Vector3.zero;
    private Quaternion netRotation = Quaternion.identity;

    private Dictionary<string, System.Action<bool>> actionMap;
    private readonly Dictionary<string, float> pulseExpireTimes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> expiredKeys = new List<string>();

    public CharacterActions CharacterActions => characterActions;
    public Vector3 NetPosition => netPosition;
    public Quaternion NetRotation => netRotation;

    private void Awake()
    {
        InitializeActions();
        BuildActionMap();
    }

    private void OnEnable()
    {
        characterActions.Reset();
        TryBindClient();
    }

    private void Update()
    {
        TryBindClient();
        UpdatePulseActions();
    }

    private void InitializeActions()
    {
        if (actionsInitialized)
        {
            return;
        }

        characterActions.InitializeActions();
        characterActions.Reset();
        actionsInitialized = true;
    }

    private void BuildActionMap()
    {
        actionMap = new Dictionary<string, System.Action<bool>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Jump", value => characterActions.jump.value = value },
            { "Run", value => characterActions.run.value = value },
            { "Interact", value => characterActions.interact.value = value },
            { "Roll", value => characterActions.roll.value = value },
            { "Lock", value => characterActions.@lock.value = value },
            { "Attack", value => characterActions.attack.value = value },
            { "HeavyAttack", value => characterActions.heavyAttack.value = value },
            { "Crouch", value => characterActions.crouch.value = value },
            { "OpenUI", value => characterActions.OpenUI.value = value },
            { "OpenConsole", value => characterActions.OpenConsoleUI.value = value },
            { "OpenConsoleUI", value => characterActions.OpenConsoleUI.value = value },
        };
    }

    private void TryBindClient()
    {
        if (registered && client != null)
        {
            return;
        }

        if (stateSyncMgr == null)
        {
            stateSyncMgr = FindObjectOfType<StateSyncMgr>();
        }

        if (stateSyncMgr == null)
        {
            return;
        }

        var newClient = stateSyncMgr.Client;
        if (newClient == null || newClient == client)
        {
            return;
        }

        client = newClient;
        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        if (registered || client == null)
        {
            return;
        }

        client.RegisterHandle(1, OnMoveMessage);
        client.RegisterHandle(2, OnActionMessage);
        registered = true;
    }

    private void OnMoveMessage(DefaultNetWorkPackage package)
    {
        var state = package?.MsgObj as CharacterState;
        if (state == null)
        {
            return;
        }

        netPosition = NetWorkUtility.ToUnityV3(state.Position);
        characterActions.movement.value = new Vector2(state.MovementX, state.MovementY);
        ApplyActionState(state);
    }

    private void ApplyActionState(CharacterState state)
    {
        characterActions.jump.value = state.Jump;
        characterActions.run.value = state.Run;
        characterActions.interact.value = state.Interact;
        characterActions.roll.value = state.Roll;
        characterActions.@lock.value = state.Lock;
        characterActions.attack.value = state.Attack;
        characterActions.heavyAttack.value = state.HeavyAttack;
        characterActions.crouch.value = state.Crouch;
        characterActions.OpenUI.value = state.OpenUI;
        characterActions.OpenConsoleUI.value = state.OpenConsoleUI;
    }

    private void OnActionMessage(DefaultNetWorkPackage package)
    {
        var action = package?.MsgObj as ActionMessage;
        if (action == null)
        {
            return;
        }

        ApplyAction(action.Actionname);
    }

    private void ApplyAction(string rawName)
    {
        if (!TryParseActionName(rawName, out string actionName, out bool value, out bool isPulse))
        {
            return;
        }

        if (!actionMap.TryGetValue(actionName, out var setter))
        {
            Debug.LogWarning($"未识别的网络动作: {actionName}");
            return;
        }

        setter(value);

        if (isPulse && actionPulseDuration > 0f)
        {
            pulseExpireTimes[actionName] = Time.time + actionPulseDuration;
        }
    }

    private static bool TryParseActionName(string rawName, out string actionName, out bool value, out bool isPulse)
    {
        actionName = rawName?.Trim();
        value = true;
        isPulse = true;

        if (string.IsNullOrEmpty(actionName))
        {
            return false;
        }

        if (actionName.EndsWith("Down", StringComparison.OrdinalIgnoreCase))
        {
            actionName = actionName.Substring(0, actionName.Length - 4);
            value = true;
            isPulse = false;
            return true;
        }

        if (actionName.EndsWith("Up", StringComparison.OrdinalIgnoreCase))
        {
            actionName = actionName.Substring(0, actionName.Length - 2);
            value = false;
            isPulse = false;
            return true;
        }

        int separatorIndex = actionName.IndexOf(':');
        if (separatorIndex < 0)
        {
            separatorIndex = actionName.IndexOf('=');
        }

        if (separatorIndex > 0 && separatorIndex < actionName.Length - 1)
        {
            string tail = actionName.Substring(separatorIndex + 1).Trim();
            if (tail == "1" || tail == "0")
            {
                value = tail == "1";
                isPulse = false;
                actionName = actionName.Substring(0, separatorIndex);
                return true;
            }
        }

        return true;
    }

    private void UpdatePulseActions()
    {
        if (pulseExpireTimes.Count == 0)
        {
            return;
        }

        float now = Time.time;
        expiredKeys.Clear();

        foreach (var pair in pulseExpireTimes)
        {
            if (pair.Value <= now)
            {
                expiredKeys.Add(pair.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            if (actionMap.TryGetValue(key, out var setter))
            {
                setter(false);
            }

            pulseExpireTimes.Remove(key);
        }
    }
}
