using BT;
using UnityEngine;

public class BTMoveAction<TKey, TValue> : BTAction<TKey, TValue>
{
    private Transform targetTransform;
    private float speed;
    private float distanceSqr = 10;
    private CharacterActor actor;

    public BTMoveAction(string name, float speed)
    {
        this.speed = speed;
    }
    public override void Activate(DataBase<TKey, TValue> database)
    {
        base.Activate(database);

        actor = database.GetData<CharacterActor>((dynamic)"actor");
    }
    protected override void Enter()
    {
        base.Enter();
        targetTransform = database.GetData<Transform>((dynamic)"target");

        //获得下一个目标点
    }

    protected override BTResult Execute()
    {
        Vector3 pos = actor.Position;
        Vector3 targetpos = targetTransform.position;
        if ((pos - targetpos).sqrMagnitude > 10000)
        {
            return BTResult.Failed;
        }
        if ((pos - targetpos).sqrMagnitude > distanceSqr)
        {
            Vector3 dir = targetpos - pos;
            dir.Normalize();
            Quaternion quaternion = Quaternion.LookRotation(dir);
            //pos += dir * speed * Time.deltaTime;
            actor.Velocity = dir * speed;
            actor.Rotation = quaternion;
        }
        else
        {
            return BTResult.Success;
        }
        return BTResult.Running;
    }
    protected override void Exit()
    {
        actor.Velocity = Vector3.zero;
        base.Exit();
    }
}
