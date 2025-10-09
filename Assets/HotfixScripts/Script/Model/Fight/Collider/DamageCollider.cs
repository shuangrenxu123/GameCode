using System.Collections.Generic;
using Fight;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField]
    Collider damageCollider;
    [SerializeField, ReadOnly]
    CombatEntity entity;
    List<Enemy.Enemy> characterDamagedDuringThisCalculation;
    LayerMask enemyLayer;
    public string EnemyTag;
    private void Awake()
    {
        entity = GetComponentInParent<CombatEntity>();

        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
        characterDamagedDuringThisCalculation = new List<Enemy.Enemy>();

        enemyLayer = LayerMask.NameToLayer("Damageable Character");
        //weaponItem = GetComponentInParent<PlayerInventory>().rightWeapon as WeaponItemData;
    }
    public void EnableDamageCollider()
    {
        entity = GetComponentInParent<CombatEntity>();
        damageCollider.enabled = true;

    }
    public void DisableDamageCollider()
    {
        characterDamagedDuringThisCalculation.Clear();
        damageCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyLayer)
        {
            var enemy = other.gameObject.GetComponentInParent<Enemy.Enemy>();
            if (characterDamagedDuringThisCalculation.Contains(enemy) || !enemy.CompareTag(EnemyTag))
            {
                return;
            }
            characterDamagedDuringThisCalculation.Add(enemy);
            var target = other.gameObject.GetComponentInParent<CombatEntity>();
            Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            float directionHitFrom = (Vector3.SignedAngle(entity.transform.forward, enemy.transform.forward, Vector3.up));

            // new DamageAction(entity, new List<CombatEntity> { target }).Apply(10);
            CombatActionFactor
               .CreateActionAndExecute<DamageAction>(entity, new List<CombatEntity> { target }, 10);

        }
    }
}
