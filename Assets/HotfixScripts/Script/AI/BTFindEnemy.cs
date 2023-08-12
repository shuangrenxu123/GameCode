using BT;
using UnityEngine;

public class BTFindEnemy : BTAction
{
    private int layer;
    private string setDataName;
    private Transform transform;
    public BTFindEnemy(string name, string setDataName, string layer, Transform transform) : base(name)
    {
        this.setDataName = setDataName;
        this.layer = LayerMask.NameToLayer(layer);
        this.transform = transform;

    }
    protected override BTResult Execute()
    {
        var colliders = Physics.OverlapSphere(transform.position, 10, 1 << layer);

        if (colliders.Length == 0 || colliders == null)
        {
            return BTResult.Failed;
        }
        else
        {
            Collider target = null;
            foreach (var collider in colliders)
            {
                var dir = collider.transform.position - transform.position;
                dir.Normalize();
                var angle = Vector3.Angle(transform.forward, dir);
                if (angle < 60 && angle > -60)
                {
                    target = collider;
                    database.SetData(setDataName, target.transform);
                    return BTResult.Success;
                }
            }
        }
        return BTResult.Failed;
    }
}
