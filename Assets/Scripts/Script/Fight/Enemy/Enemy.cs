using Fight;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public CombatEntity entity;
    private void Start()
    {
        entity = GetComponent<CombatEntity>();
        entity.Init(100);
    }
}
