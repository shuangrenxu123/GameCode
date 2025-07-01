using UnityEngine;

namespace BT
{
    /// <summary>
    /// 最终的执行的节点，只能为叶子节点
    /// </summary>
    public class BTAction : BTNode
    {
        private BTActionStatus status = BTActionStatus.Ready;

        /// <summary>
        /// 在操作节点即将执行时调用。
        /// </summary>
        protected virtual void Enter()
        {
        }

        /// <summary>
        /// 在操作节点完成时调用。
        /// </summary>
        protected virtual void Exit()
        {
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
        /// <summary>
        /// 获得运行状态，一般不需要重写该方法
        /// </summary>
        /// <returns></returns>
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
                    isRunning = false;
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