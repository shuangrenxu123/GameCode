using UnityEngine;

namespace BT
{
    public class BTAction : BTNode
    {
        public Transform transform;
        private BTActionStatus status = BTActionStatus.Ready;
        public BTAction(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// 在操作节点即将执行时调用。
        /// </summary>
        protected virtual void Enter()
        {
            Debug.Log("进入了:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        /// <summary>
        /// 在操作节点完成时调用。
        /// </summary>
        protected virtual void Exit()
        {
            Debug.Log("退出了:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        public override void Clear()
        {
            base.Clear();
            if (status != BTActionStatus.Ready)
            {
                Exit();
                status = BTActionStatus.Ready;
            }
        }
        public override BTResult Tick()
        {
            BTResult result = BTResult.Success;
            if (status == BTActionStatus.Ready)
            {
                Enter();
                status = BTActionStatus.Running;
                isRunning = false;
            }
            if (status == BTActionStatus.Running)
            {
                result = Execute();
                if (result != BTResult.Running)
                {
                    Exit();
                    status = BTActionStatus.Ready;
                    isRunning =false;
                }
            }
            return result;
        }
        /// <summary>
        /// 运行时每帧调用
        /// </summary>
        protected virtual BTResult Execute() { return BTResult.Failed; }

    }
    public enum BTActionStatus
    {
        Ready = 1,
        Running = 2,
    }
}