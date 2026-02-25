using Character.Player;
using Game.Net.Entities;
using Network;
using Network.Tcp;
using PlayerInfo;
using UnityEngine;

public class StateSyncMgr : MonoBehaviour
{
    #region setting
    /// <summary>
    /// 当超过该值的时候会被直接拉过去
    /// </summary>
    public static readonly int MaxDisplacement = 10;
    #endregion
    public Player player;
    public GameObject prefab;
    TcpClient client;
    public TcpClient Client => client;
    private readonly NetEntityRegistry entityRegistry = new NetEntityRegistry();
    private NetSpawner spawner;
    private void Start()
    {
        EnsureLocalPlayerReference();
        EnsureLocalPlayerId();
        spawner = new NetSpawner(prefab);

        var a = new NetWorkManager.CreateParameters();
        a.PackageCoderType = typeof(DefaultNetworkPackageCoder);
        a.PackageBodyCoderType = typeof(ProtobufCoder);

        client = Network.NetWorkManager.Instance.CreateTcpClient(a);

        client.ConnectServer("127.0.0.1", 9000);
        client.RegisterHandle(1, SyncGameObjectState);
    }

    /// <summary>
    /// 同步坐标位置
    /// </summary>
    /// <param name="package"></param>
    void SyncGameObjectState(DefaultNetWorkPackage package)
    {
        if (package == null)
        {
            return;
        }

        if (package.MsgId == 1)
        {
            var state = package.MsgObj as CharacterState;
            if (state == null)
            {
                return;
            }

            HandleCharacterState(package.SenderId, state, package);
            return;
        }

    }
    public void SendCharacterState(Vector3 position, CharacterActions actions)
    {
        if (client == null || client.state != ENetWorkState.Connected)
        {
            return;
        }

        string senderId = player != null ? player.id : "0";
        float movementX = 0f;
        float movementY = 0f;
        bool jump = false;
        bool run = false;
        bool interact = false;
        bool roll = false;
        bool @lock = false;
        bool attack = false;
        bool heavyAttack = false;
        bool crouch = false;
        bool openUI = false;
        bool openConsoleUI = false;

        if (actions != null)
        {
            movementX = actions.movement.value.x;
            movementY = actions.movement.value.y;
            jump = actions.jump.value;
            run = actions.run.value;
            interact = actions.interact.value;
            roll = actions.roll.value;
            @lock = actions.@lock.value;
            attack = actions.attack.value;
            heavyAttack = actions.heavyAttack.value;
            crouch = actions.crouch.value;
            openUI = actions.OpenUI.value;
            openConsoleUI = actions.OpenConsoleUI.value;
        }

        var message = new CharacterState
        {
            Position = NetWorkUtility.ToProtoBufV3(position),
            MovementX = movementX,
            MovementY = movementY,
            Jump = jump,
            Run = run,
            Interact = interact,
            Roll = roll,
            Lock = @lock,
            Attack = attack,
            HeavyAttack = heavyAttack,
            Crouch = crouch,
            OpenUI = openUI,
            OpenConsoleUI = openConsoleUI,
        };

        client.SendMessage(senderId, 1, message);
    }

    public void SimulateCharacterState(string senderId, CharacterState state)
    {
        if (state == null)
        {
            return;
        }

        HandleCharacterState(senderId, state, null);
    }

    private NetEntity SpawnEntity(string name)
    {
        if (spawner == null)
        {
            spawner = new NetSpawner(prefab);
        }

        return spawner.Spawn(name);
    }

    private void HandleCharacterState(string senderId, CharacterState state, DefaultNetWorkPackage package)
    {
        if (IsLocalSender(senderId))
        {
            return;
        }

        if (entityRegistry.TryGet(senderId, out var entity))
        {
            ApplyCharacterState(entity, state, package);
        }
        else
        {
            var newEntity = SpawnEntity(senderId);
            if (newEntity != null)
            {
                entityRegistry.Register(newEntity);
                ApplyCharacterState(newEntity, state, package);
            }
        }
    }

    private static void ApplyCharacterState(NetEntity entity, CharacterState state, DefaultNetWorkPackage package)
    {
        if (entity == null || state == null)
        {
            return;
        }

        var netObj = entity.GetComponent<NetObj>();
        if (netObj != null && package != null)
        {
            netObj.SyncData(package);
            return;
        }

        entity.transform.position = NetWorkUtility.ToUnityV3(state.Position);
    }

    private bool IsLocalSender(string senderId)
    {
        return player != null && senderId == player.id;
    }

    private void EnsureLocalPlayerReference()
    {
        if (player != null)
        {
            return;
        }

        player = Player.Instance;
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
    }

    private void EnsureLocalPlayerId()
    {
        if (player == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(player.id))
        {
            player.id = System.Guid.NewGuid().ToString("N");
        }
    }
}
