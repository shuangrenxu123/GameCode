using UnityEngine;

namespace BT
{
    public class BTAction : BTNode
    {
        private BTActionStatus status = BTActionStatus.Ready;

        public BTAction(BTPrecondition precondition = null) : base(precondition) { }

        protected virtual void Enter()
        {
            Debug.Log("������:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        protected virtual void Exit()
        {
            Debug.Log("�˳���:" + this.name + " [" + this.GetType().ToString() + "]");
        }
        /// <summary>
        /// ִ�к���
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
        /// ���״̬�������ж������¿�ʼִ���أ����Ǽ���ִ��
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
            //���ﲻʹ��else ��Ϊ������������һ��
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