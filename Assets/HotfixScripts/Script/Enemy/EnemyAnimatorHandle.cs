using UnityEngine;

public class EnemyAnimatorHandle : AnimatorManager
{
    public Enemy enemy;
    DamageCollider damageCollider;
    public override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
        damageCollider = GetComponentInChildren<DamageCollider>();
    }
    private void OnAnimatorMove()
    {
        Vector3 deltaPosition = anim.deltaPosition;
        enemy.characterController.Move(deltaPosition);
        enemy.transform.rotation *= anim.deltaRotation;
    }

    public void EnableCollider()
    {
        damageCollider.EnableDamageCollider();
    }
    public void DisableCollider()
    {
        damageCollider.DisableDamageCollider();
    }
}
