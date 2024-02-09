using Fight;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    Collider damageCollider;
    CombatEntity entity;
    //WeaponItemData weaponItem;
    //List<CharacterManager> characterDamagedDuringThisCalculation;
    public string currentDamageAnimation;
    LayerMask Enemylayer;
    public string EnemyTag;
    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
        //characterDamagedDuringThisCalculation = new List<CharacterManager>();
        Enemylayer = LayerMask.NameToLayer("Damageable Character");
        //weaponItem = GetComponentInParent<PlayerInventory>().rightWeapon as WeaponItemData;
    }
    public void EnableDamageCollider()
    {
        entity = GetComponentInParent<CombatEntity>();
        damageCollider.enabled = true;
        //characterDamagedDuringThisCalculation.Clear();
    }
    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Enemylayer)
        {
            Enemy enemy = other.gameObject.GetComponentInParent<Enemy>();
           //if (characterDamagedDuringThisCalculation.Contains(enemy) || !enemy.CompareTag(EnemyTag))
           //{
           //    return;
           //}
           //characterDamagedDuringThisCalculation.Add(enemy);
            var target = other.gameObject.GetComponentInParent<CombatEntity>();
            //Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            //float directionHitFrom = (Vector3.SignedAngle(entity.transform.forward, enemy.transform.forward, Vector3.up));
            //ChooseWhichDirectionDamageCameFrom(directionHitFrom);
            //new DamageAction(entity, new CombatEntity[] { target }) { animator = currentDamageAnimation }.Apply(10);
        }
    }
    protected virtual void ChooseWhichDirectionDamageCameFrom(float direction)
    {
        if (direction >= 145 && direction <= 180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }
        else if (direction <= -145 && direction >= -180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }
        else if (direction >= -45 && direction <= 45)
        {
            currentDamageAnimation = "Damage_Back_01";
        }
        else if (direction >= -144 && direction <= -45)
        {
            currentDamageAnimation = "Damage_Left_01";
        }
        else if (direction >= 45 && direction <= 144)
        {
            currentDamageAnimation = "Damage_Right_01";
        }
    }
}
