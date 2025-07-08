using CharacterControllerStateMachine;
using Fight;
using UnityEngine;
using static Fight.Number.CombatNumberBox;

[RequireComponent(typeof(CharacterActor))]
[RequireComponent(typeof(PlayerInventory))]
public class Player : MonoBehaviour
{
    public CombatEntity CombatEntity { get; private set; }
    public NetTranform Net { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    public CharacterActor Actor { get; private set; }
    public StateManger StateManager { get; private set; }

    [Header("Net")]
    public string id;
    private void Awake()
    {
        CombatEntity = GetComponent<CombatEntity>();
        Net = GetComponent<NetTranform>();
        Inventory = GetComponent<PlayerInventory>();
        Actor = GetComponent<CharacterActor>();
        StateManager = GetComponentInChildren<StateManger>();
        //backStep = GetComponentInChildren<BackStepCollider>();
        //UIManager.Instance.OpenUI<PlayerStateUI>(UnityEngine.Resources.Load<PlayerStateUI>("playerState"));
        //var ui = ResourcesManager.Instance.LoadAsset<PlayerStateUI>("ui","playerState.prefab");
        //UIManager.Instance.OpenUI<PlayerStateUI>(ui);
    }

    void Start()
    {
        CombatEntity.hp.SetMaxValue(100);
        CombatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
        CombatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
    }

    public void SetStateMachineData(string key, object value)
    {
        StateManager.SetStateMachineData(key, value);
    }

}
public struct MotionState
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 velocity;
    public float lastMotionTime;
}
