using AIBlackboard;
using BT;
using CharacterController;
using UnityEngine;

public class BTRandomTargetPosition : BTAction
{
    private CharacterActor actor;
    private Transform target;
    public string setDataName;

    public Vector2 Range;
    public BTRandomTargetPosition()
    {
        target = new GameObject("target").transform;

    }
    public override void Activate(Blackboard database)
    {
        base.Activate(database);
        actor = database.GetValue<CharacterActor>((dynamic)"actor");
        database.SetValue(setDataName, (dynamic)target);
    }
    protected override BTResult Execute()
    {
        Vector3 pos = Vector3.zero;
        target.position = actor.Position;
        Vector3 ray;
        do
        {
            pos.x = Random.Range(-Range.x, Range.x);
            pos.z = Random.Range(-Range.y, Range.y);
            ray = pos;
        } while (Physics.Raycast(actor.Position, ray, 10f));
        target.position = actor.Position + pos;
        return BTResult.Success;
    }
    public override void Clear()
    {
        base.Clear();
    }
}
