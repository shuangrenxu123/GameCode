using BT;
using UnityEngine;
public class DoRun : BTAction
{
    private string destinationName;
    private int destinationDataId;

    private Vector3 destination;
    /// <summary>
    /// 距离差
    /// </summary>
    private float tolerance = 0.1f;
    private float speed;

    private Transform trans;

    public DoRun(string name, float sp)
    {
        destinationName = name;
        speed = sp;
    }

    public override void Activate(BTDataBase database)
    {
        base.Activate(database);
        destinationDataId = database.GetDataId(destinationName);
        trans = database.transform;
    }

    protected override BTResult Execute()
    {
        UpdateDestination();
        UpdateFaceDirection();

        if (CheckArrived())
        {
            return BTResult.Ended;
        }
        MoveToDestination();
        return BTResult.Running;
    }
    /// <summary>
    /// 更新距离
    /// </summary>
    /// <returns></returns>
    private void UpdateDestination()
    {
        destination = database.GetData<Vector3>(destinationDataId);
    }

    /// <summary>
    /// 更新朝向
    /// </summary>
    private void UpdateFaceDirection()
    {
        Vector3 offset = destination - trans.position;
        if (offset.x >= 0)
        {
            trans.localEulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            trans.localEulerAngles = Vector3.zero;
        }
    }

    private bool CheckArrived()
    {
        Vector3 offset = destination - trans.position;
        return offset.magnitude <= tolerance * tolerance;
    }

    private void MoveToDestination()
    {
        Vector3 direction = (destination - trans.position).normalized;
        trans.position += direction * speed;
    }
}
