using UnityEngine;

namespace BT
{
    public class BTAction : BTNode
    {
        private BTActionStatus status = BTActionStatus.Ready;

        public BTAction(BTPrecondition precondition = null) : base(precondition) { }

        protected virtual void Enter()
        {
            Debug.Log("进入了:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        protected virtual void Exit()
        {
            Debug.Log("退出了:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        /// <summary>
        /// 执行函数
        /// </summary>
        /// <returns></returns>
        protected virtual BTResult Execute()
        {
            return BTResult.Running;
        }
        public override void Clear()
        {
            if (status != BTActionStatus.Ready)
            {
                Exit();
                status = BTActionStatus.Ready;
            }
        }
        /// <summary>
        /// 获得状态，用于判断是重新开始执行呢，还是继续执行
        /// </summary>
        /// <returns></returns>
        public override BTResult Tick()
        {
            BTResult result = BTResult.Ended;
            if (status == BTActionStatus.Ready)
            {
                Enter();
                status = BTActionStatus.Running;
            }
            //这里不使用else 是为了立即进行下一步
            if (status == BTActionStatus.Running)
            {
                result = Execute();
                if (result != BTResult.Running)
                {
                    Exit();
                    status = BTActionStatus.Ready;
                }
            }
            return result;
        }
        public override void AddChild(BTNode aNode)
        {
            Debug.LogError("BTAction: Cannot add a node into BTAction.");
        }

        public override void RemoveChild(BTNode aNode)
        {
            Debug.LogError("BTAction: Cannot remove a node into BTAction.");
        }
    }
    public enum BTActionStatus
    {
        Ready = 1,
        Running = 2,
    }
}