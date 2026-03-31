using CharacterController;
using CharacterController.Camera;
using CharacterControllerStateMachine;
using Fight;
using Fight.Number;
using UnityEngine;

namespace Character.Player
{
    [RequireComponent(typeof(CharacterActor))]
    public class Player : MonoSingleton<Player>
    {
        public CombatEntity CombatEntity { get; private set; }
        public PlayerInventory Inventory { get; private set; }
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
            CombatEntity.properties.BeginBatch();
            CombatEntity.properties.RegisterProperty(PropertyType.MaxHp, 100, 1, 999999);
            CombatEntity.properties.RegisterProperty(PropertyType.Attack, 10, 0, 999999);
            CombatEntity.properties.RegisterProperty(PropertyType.Defense, 10, 0, 999999);
            CombatEntity.properties.RegisterProperty(PropertyType.SpeedMultiplier, 100, 0, 10000);
            CombatEntity.properties.RegisterProperty(PropertyType.RotationMultiplier, 100, 0, 10000);
            CombatEntity.properties.EndBatch();

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
