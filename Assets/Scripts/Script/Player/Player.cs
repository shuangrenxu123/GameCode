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
}
