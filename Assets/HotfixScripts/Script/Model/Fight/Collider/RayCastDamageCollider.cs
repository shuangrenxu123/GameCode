using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fight;
public class RayCastDamageCollider : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField]
    Transform pointA;

    [SerializeField]
    Transform pointB;
    [SerializeField, Range(1, 5)]
    int pointCount = 5;
    [SerializeField, Range(0, 5)]
    int transformLerpCount = 3;
    Dictionary<int, Vector3> lastPointPos = new();
    [SerializeField]
    LayerMask layer;

    [SerializeField]
    CombatEntity entity;

    //runtime
    RaycastHit[] raycastHits = new RaycastHit[6];
    List<Enemy> characterDamagedDuringThisCalculation = new();
    [SerializeField]
    bool attacking = false;
    Vector3 lastTransformPos;
    Quaternion lastTransformRotation;

    public void EnableDamageCollider()
    {
        for (int i = 0; i <= pointCount; i++)
        {
            var point = Vector3.Lerp(pointA.position, pointB.position, (float)i / pointCount);
            lastPointPos[i] = point;
        }
        attacking = true;

        lastTransformPos = transform.position;
        lastTransformRotation = transform.rotation;

    }
    public void DisableDamageCollider()
    {
        characterDamagedDuringThisCalculation.Clear();
        attacking = false;
    }

    void Update()
    {
        if (attacking == false)
            return;

        var newA = pointA.position;
        var newB = pointB.position;
        Debug.DrawLine(newA, newB, Color.red, 1f);

        Ray();

    }


    void Ray()
    {
        for (int i = 0; i <= pointCount; i++)
        {
            var point = Vector3.Lerp(pointA.position, pointB.position, (float)i / pointCount);

            lastPointPos.TryGetValue(i, out Vector3 lastPos);

            Debug.DrawRay(lastPos, point - lastPos, Color.blue, 1f);

            var ray = new Ray(lastPos, point - lastPos);

            var count = Physics.RaycastNonAlloc
                (ray, raycastHits,
                 Vector3.Distance(lastPos, point),
                 layer,
                 QueryTriggerInteraction.Ignore);

            for (int j = 0; j < count; j++)
            {
                var target = raycastHits[j].collider.GetComponentInParent<CombatEntity>();
                Enemy enemy = target.gameObject.GetComponentInParent<Enemy>();
                if (!characterDamagedDuringThisCalculation.Contains(enemy))
                {
                    characterDamagedDuringThisCalculation.Add(enemy);
                    new DamageAction(entity, new List<CombatEntity> { target }).Apply(10);
                }
            }


            lastPointPos[i] = point;

        }
    }
}
