using BT;
using UnityEngine;

public class BTFindEnemy<TKey, TValue> : BTAction<TKey, TValue>
{
    private int layer;
    public TKey setDataName;
    private CharacterActor actor;
    public BTFindEnemy(string name, string layer)
    {
        this.layer = LayerMask.NameToLayer(layer);
    }
    public override void Activate(DataBase<TKey, TValue> database)
    {
        base.Activate(database);
        actor = database.GetData<CharacterActor>((dynamic)"actor");
    }
    protected override BTResult Execute()
    {
        var colliders = Physics.OverlapSphere(actor.Position, 10, 1 << layer);

        if (colliders.Length == 0 || colliders == null)
        {
            return BTResult.Failed;
        }
        else
        {
            Collider target = null;
            foreach (var collider in colliders)
            {
                var dir = collider.transform.position - actor.Position;
                dir.Normalize();
                var angle = Vector3.Angle(actor.Forward, dir);
                if (angle < 60 && angle > -60)
                {
                    target = collider;
                    database.SetData(setDataName, (dynamic)target.transform);
                    return BTResult.Success;
                }
            }
        }
        return BTResult.Failed;
    }
}
