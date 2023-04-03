using BT;
using UnityEngine;

public class BTRandomTargetPosition : BTAction
{
    private Transform transform;
    private string setDataName;
    public BTRandomTargetPosition(string name,Transform transform)
    {
        this.transform = transform;
        setDataName = name;
    }
    protected override BTResult Execute()
    {
        var pos = transform.position;
        pos.x += Random.Range(-20, 20);
        pos.z += Random.Range(-20, 20);
        database.SetData(setDataName,pos);
        return BTResult.Success;
    }
}
