using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTMoveAction : BTAction
{
    private Vector3 targetTransform;
    private float speed;
    private string readDataName;
    private float distanceSqr;
    private Transform trans;

    public BTMoveAction(Transform trans,float speed,string name,float distanceSqr)
    {
        this.trans = trans;
        this.speed = speed;
        readDataName = name;
        this.distanceSqr = distanceSqr;
    }
    protected override void Enter()
    {
        base.Enter();
        //获得下一个目标点
        targetTransform = database.GetData<Vector3>(readDataName);
    }
    protected override BTResult Execute()
    {
        Vector3 pos = trans.position;
        if ((pos - targetTransform).sqrMagnitude > distanceSqr)
        {
            Vector3 dir = targetTransform - pos;
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
