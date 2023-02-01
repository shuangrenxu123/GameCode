using BT;
using UnityEngine;
/// <summary>
/// 检查是否在视野（范围）内的前提条件
/// </summary>
public class CheckInSight : BTPrecondition
{
    private float sightLength;

    private string targetName;
    Transform trans;
    public CheckInSight(float sightLength, string targetName)
    {
        this.sightLength = sightLength;
        this.targetName = targetName;
    }
    public override void Activate(BTDataBase database)
    {
        base.Activate(database);
        trans = database.transform;
    }
    public override bool Check()
    {
        GameObject target = GameObject.Find(targetName);
        if (target == null)
        {
            return false;
        }
        Vector3 offset = target.transform.position - trans.position;
        return offset.sqrMagnitude <= sightLength * sightLength;
    }
}
