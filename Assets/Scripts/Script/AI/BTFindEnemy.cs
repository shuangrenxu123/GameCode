using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTFindEnemy : BTAction
{
    private string tag;
    private string setDataName;
    private Transform transform;
    public BTFindEnemy(string setDataName, string tag, Transform transform)
    {
        this.setDataName = setDataName;
        this.tag = tag;
        this.transform = transform;

    }
    protected override BTResult Execute()
    {
        var colliders = Physics.OverlapSphere(transform.position, 10);
        if (colliders.Length == 0 || colliders == null)
        {
            return BTResult.Success;
        }
        else
        {
            Collider target = null;
            foreach (var collider in colliders)
            {
                var dir = collider.transform.position - transform.position;
                dir.Normalize();
                var angle = Vector3.Angle(transform.position, dir);
                if (angle < 60 || angle > -60)
                {
                    target = collider;
                    database.SetData(setDataName, target.transform.position);
                    return BTResult.Success;
                }
            }
        }
        return BTResult.Success;
    }
}
