using UnityEngine;
namespace BT
{


    public class FindTarget : BTAction
    {
        private string targetName;
        protected int destinationDataid;
        private string destinationDataName;

        protected Transform trans;

        public FindTarget(string targetName, string dataName, BTPrecondition precondition = null) : base(precondition)
        {
            this.targetName = targetName;
            destinationDataName = dataName;
        }

        public override void Activate(BTDataBase database)
        {
            base.Activate(database);
            trans = database.transform;
            destinationDataid = database.GetDataId(destinationDataName);
        }
        protected Vector3 GetToTargetOffset()
        {
            GameObject target = GameObject.Find(targetName);
            if (target == null)
            {
                return Vector3.zero;
            }
            return target.transform.position - trans.position;
        }
        protected bool CheckTarget()
        {
            return GameObject.Find(targetName) != null;
        }
    }

    /// <summary>
    /// 寻找远离目标
    /// </summary>
    public class FindEscapeDestination : FindTarget
    {
        /// <summary>
        /// 对于目标可接受的距离
        /// </summary>
        private float distance;
        public FindEscapeDestination(string targetName, string destinationDataName, float dis, BTPrecondition precondition = null) : base(targetName, destinationDataName, precondition)
        {
            distance = dis;
        }

        protected override BTResult Execute()
        {
            if (!CheckTarget())
            {
                return BTResult.Ended;
            }
            Vector3 offset = GetToTargetOffset();
            if (offset.sqrMagnitude <= distance * distance)
            {
                Vector3 direction = -offset.normalized;
                Vector3 destination = distance * direction * Random.Range(1.2f, 1.3f);
                database.SetData<Vector3>(destinationDataid, destination);
                return BTResult.Running;
            }
            return BTResult.Ended;
        }
    }
    /// <summary>
    /// 寻找目标Action
    /// </summary>
    public class FindToTargetDestination : FindTarget
    {
        private float _minDistance;

        public FindToTargetDestination(string targetName, string destinationDataName, float minDistance, BTPrecondition precondition = null) : base(targetName, destinationDataName, precondition)
        {
            _minDistance = minDistance;
        }

        protected override BTResult Execute()
        {
            if (!CheckTarget())
            {
                return BTResult.Ended;
            }

            Vector3 offset = GetToTargetOffset();
            if (offset.sqrMagnitude >= _minDistance * _minDistance)
            {
                Vector3 direction = offset.normalized;
                Vector3 destination = trans.position + offset - _minDistance * direction;
                database.SetData<Vector3>(destinationDataid, destination);
                return BTResult.Running;
            }
            return BTResult.Ended;
        }
    }
}
