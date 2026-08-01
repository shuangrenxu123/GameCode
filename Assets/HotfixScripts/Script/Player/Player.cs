using CharacterController;
using CharacterController.Camera;
using CharacterControllerStateMachine;
using Fight;
using GameSave;
using UnityEngine;

namespace Character.Player
{
    [RequireComponent(typeof(CharacterActor))]
    public class Player : MonoSingleton<Player>
    {
        PlayerSaveData data;
        PlayerInventory inventory;

        public CombatEntity CombatEntity { get; private set; }
        public PlayerSaveData Data
        {
            get
            {
                EnsureInventoryInitialized();
                return data;
            }
        }
        public PlayerInventory Inventory
        {
            get
            {
                EnsureInventoryInitialized();
                return inventory;
            }
        }
        public CharacterActor Actor { get; private set; }
        public CharacterBrain brain { get; private set; }
        public StateManger StateManager { get; private set; }
        public Camera3D camera3D;

        [Header("Net")]
        public string id;

        protected override void Awake()
        {
            base.Awake();
            brain = GetComponentInChildren<CharacterBrain>();
            CombatEntity = GetComponent<CombatEntity>();
            Actor = GetComponent<CharacterActor>();
            StateManager = GetComponentInChildren<StateManger>();
        }

        void Start()
        {
            EnsureInventoryInitialized();
        }

        void EnsureInventoryInitialized()
        {
            if (inventory != null)
            {
                return;
            }

            data = GameSaveManager.Instance.PlayerData;
            if (string.IsNullOrWhiteSpace(data.playerId))
            {
                data.playerId = id;
            }
            else
            {
                id = data.playerId;
            }

            inventory = new PlayerInventory(data);
            GameSaveManager.Instance.BeforeSave += SyncSaveData;
            GameSaveManager.Instance.PlayerDataLoaded += ApplyLoadedData;
        }

        void SyncSaveData()
        {
            data.playerId = id;
        }

        void ApplyLoadedData()
        {
            if (!string.IsNullOrWhiteSpace(data.playerId))
            {
                id = data.playerId;
            }

            inventory.RefreshLoadedData();
        }

        void OnDestroy()
        {
            if (inventory == null)
            {
                return;
            }

            GameSaveManager.Instance.BeforeSave -= SyncSaveData;
            GameSaveManager.Instance.PlayerDataLoaded -= ApplyLoadedData;
        }

    }
    public struct MotionState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 velocity;
        public float lastMotionTime;
    }
}
