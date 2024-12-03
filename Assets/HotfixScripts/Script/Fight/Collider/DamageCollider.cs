using Fight;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [SerializeField]
    Collider damageCollider;
    [SerializeField]
    CombatEntity entity;
    WeaponItemData weaponItem;
    List<Enemy> characterDamagedDuringThisCalculation;
    private string currentDamageAnimation;
    LayerMask Enemylayer;
    [Tag]
    public string EnemyTag;
    private void Awake()
    {
        entity = GetComponentInParent<CombatEntity>();
        //damageCollider = GetComponent<Collider>();
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
        characterDamagedDuringThisCalculation = new List<Enemy>();
        //todo ����ȡ��Ӳ�����д�������������������ж�
        Enemylayer = LayerMask.NameToLayer("Damageable Character");
        //weaponItem = GetComponentInParent<PlayerInventory>().rightWeapon as WeaponItemData;
    }
    public void EnableDamageCollider()
    {
        //entity = GetComponentInParent<CombatEntity>();
        damageCollider.enabled = true;

    }
    public void DisableDamageCollider()
    {
        characterDamagedDuringThisCalculation.Clear();
        damageCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Enemylayer)
        {
            Enemy enemy = other.gameObject.GetComponentInParent<Enemy>();
            if (characterDamagedDuringThisCalculation.Contains(enemy) || !enemy.CompareTag(EnemyTag))
            {
                return;
            }
            Debug.Log("������:" + enemy.name);
            characterDamagedDuringThisCalculation.Add(enemy);
            var target = other.gameObject.GetComponentInParent<CombatEntity>();
            Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            float directionHitFrom = (Vector3.SignedAngle(entity.transform.forward, enemy.transform.forward, Vector3.up));
            new DamageAction(entity, new List<CombatEntity> { target }).Apply(10);
            target.TakeDamageFx(directionHitFrom);
        }
    }
}
