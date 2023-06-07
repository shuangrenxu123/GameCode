using Fight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    Player player;
    Collider damageCollider;
    CombatEntity entity;
    WeaponItemData weaponItem;
    List<CharacterManager> characterDamagedDuringThisCalculation;
    public string currentDamageAnimation;
    LayerMask Enemylayer;
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
        characterDamagedDuringThisCalculation = new List<CharacterManager>();
        Enemylayer = LayerMask.NameToLayer("Damageable Character");
    }
    private void Start()
    {
        player = GetComponentInParent<Player>();
        weaponItem = GetComponentInParent<PlayerInventory>().rightWeapon as WeaponItemData;
        entity = GetComponentInParent<CombatEntity>();
    }
    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
        characterDamagedDuringThisCalculation.Clear();
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
            if (characterDamagedDuringThisCalculation.Contains(enemy))
            {
                return;
            }
            characterDamagedDuringThisCalculation.Add(enemy);
            var target = other.gameObject.GetComponentInParent<CombatEntity>();
            //Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            float directionHitFrom = (Vector3.SignedAngle(player.transform.forward, enemy.transform.forward, Vector3.up));
            ChooseWhichDirectionDamageCameFrom(directionHitFrom);
            new DamageAction(entity, new CombatEntity[] { target }) { animator = currentDamageAnimation }.Apply(10);
            AudioManager.Instance.PlaySound(weaponItem.attackToEnemy_audios[0]);
        }
    }
    protected virtual void ChooseWhichDirectionDamageCameFrom(float direction)
    {
        if(direction >=145 && direction <= 180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }
        else if(direction <= -145 && direction >= -180)
        {
            currentDamageAnimation = "Damage_Forward_01";
        }else if(direction >= -45 && direction <= 45)
        {
            currentDamageAnimation = "Damage_Back_01";
        }
        else if(direction >= -144 && direction <= -45)
        {
            currentDamageAnimation = "Damage_Left_01";
        }
        else if(direction >= 45 && direction <= 144)
        {
            currentDamageAnimation = "Damage_Right_01";
        }
    }
}
