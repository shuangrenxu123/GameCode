using Network.Tcp;
using PlayerInfo;
using UnityEngine;

public class NetCharacterInput : MonoBehaviour
{
    [SerializeField]
    private StateSyncMgr stateSyncMgr;

    private TcpClient client;
    private bool registered;

    private CharacterActions characterActions = new CharacterActions();
    private bool actionsInitialized;

    private Vector3 netPosition = Vector3.zero;
    private Quaternion netRotation = Quaternion.identity;

    public CharacterActions CharacterActions => characterActions;
    public Vector3 NetPosition => netPosition;
    public Quaternion NetRotation => netRotation;

    private void Awake()
    {
        InitializeActions();
    }

    private void OnEnable()
    {
        characterActions.Reset();
        TryBindClient();
    }

    private void Update()
    {
        TryBindClient();
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
}
