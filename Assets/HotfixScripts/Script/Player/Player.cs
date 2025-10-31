using CharacterController;
using CharacterController.Camera;
using CharacterControllerStateMachine;
using Fight;
using UnityEngine;
using static Fight.Number.CombatNumberBox;

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
            CombatEntity.hp.SetMaxValue(100);
            CombatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
            CombatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
            CombatEntity.properties.RegisterAttribute(PropertyType.SpeedMultiplier, 100);
            CombatEntity.properties.RegisterAttribute(PropertyType.RotationMultiplier, 100);

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