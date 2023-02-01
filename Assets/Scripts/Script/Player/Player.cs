using NetWork;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CombatEntity entity;
    public PlayerController controller;
    private void Awake()
    {
        entity = GetComponent<CombatEntity>();
        controller = GetComponent<PlayerController>();
    }
    void Start()
    {
        entity.Init(1000);
    }

    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "enemy")
        {
            new DamageAction(entity, other.GetComponent<Enemy>().entity).Allpy();
        }
    }

    private void FixedUpdate()
    {
        if (NetWorkManager.Instance.state == ENetWorkState.Connected)
            SendPos();
    }
    private void SendPos()
    {
        PlayerInfo.move msg = new PlayerInfo.move
        {
            Id = 1,
            X = transform.position.x,
            Y = transform.position.y,
            Z = transform.position.z
        };
        NetWorkManager.Instance.SendMessage(2, msg);
    }
}
