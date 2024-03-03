using Fight;
using UnityEngine;

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
        //backStep = GetComponentInChildren<BackStepCollider>();
        UIManager.Instance.OpenUI<PlayerStateUI>(Resources.Load<PlayerStateUI>("playerState"));
    }
    void Start()
    {
        CombatEntity.Init(1000);
    }

}
public struct MotionState
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 velocity;
    public float lastMotionTime;
}
