using BT;
using UnityEngine;

public class BTRandomTargetPosition : BTAction
{
    private Transform transform;
    private Transform target;
    private string setDataName;
    public BTRandomTargetPosition(string name,Transform transform)
    {
        target = new GameObject("target").transform;
        this.transform = transform;
        setDataName = name;
        
    }
    protected override BTResult Execute()
    {
        Vector3 pos = Vector3.zero;
        target.position = transform.position;
        Vector3 ray;
        do
        {
            pos.x = Random.Range(-5, 5);
            pos.z = Random.Range(-5, 5);
            ray = pos;
        } while (Physics.Raycast(transform.position, ray, 10f));
        target.position = transform.position + pos;
        database.SetData(setDataName,target);
        return BTResult.Success;
    }
    public override void Clear()
    {
        base.Clear();
    }
}
