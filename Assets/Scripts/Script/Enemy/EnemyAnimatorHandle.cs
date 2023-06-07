using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class EnemyAnimatorHandle : AnimatorManager
{
    public Enemy enemy;
    public override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
    }
    private void OnAnimatorMove()
    {
        Vector3 deltaPosition = anim.deltaPosition;
        enemy.characterController.Move(deltaPosition);
        enemy.transform.rotation *= anim.deltaRotation;
    }
}
