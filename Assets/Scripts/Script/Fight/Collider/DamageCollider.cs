using Fight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    Collider damageCollider;
    CombatEntity entity;
    
    private void Awake()
    {
        damageCollider = GetComponent<Collider>();
        damageCollider.gameObject.SetActive(true);
        damageCollider.isTrigger = true;
        damageCollider.enabled = false;
    }
    public void EnableDamageCollider()
    {
        damageCollider.enabled = true;
        entity = GetComponentInParent<CombatEntity>();
    }
    public void DisableDamageCollider()
    {
        damageCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Hittable")
        {
            var target =  other.gameObject.GetComponent<CombatEntity>();
            new DamageAction(entity,target).Apply();
        }
    }
}
