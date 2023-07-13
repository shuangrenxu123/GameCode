using BT;
using UnityEngine;

public class BTMoveAction : BTAction
{
    private Transform targetTransform;
    private float speed;
    private string readDataName;
    private float distanceSqr;
    private Transform trans;

    public BTMoveAction(string name, Transform trans, float speed, string dataname, float distanceSqr) : base(name)
    {
        this.trans = trans;
        this.speed = speed;
        readDataName = dataname;
        this.distanceSqr = distanceSqr;
    }
    protected override void Enter()
    {
        base.Enter();
        //获得下一个目标点
        targetTransform = database.GetData<Transform>(readDataName);

    }
    protected override BTResult Execute()
    {
        Vector3 pos = trans.position;
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
            pos += dir * speed * Time.deltaTime;
            trans.position = pos;
            trans.rotation = quaternion;
        }
        else
        {
            return BTResult.Success;
        }
        return BTResult.Running;
    }
}
