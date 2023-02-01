using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// ���нڵ�ĸ��ڵ�
    /// </summary>
    public class BTNode
    {
        public string name;

        /// <summary>
        /// �ӽڵ�
        /// </summary>
        public List<BTNode> children;
        /// <summary>
        /// ����Ƿ���Խ���˽ڵ�
        /// </summary>
        public BTPrecondition precondition;

        public BTDataBase database;

        /// <summary>
        /// ��ȴʱ��
        /// </summary>
        public float interval = 0;
        private float lastTimeEvaluated = 0;
        /// <summary>
        /// �Ƿ��Ѿ�����
        /// </summary>
        public bool activated;

        public BTNode() : this(null)
        {

        }
        public BTNode(BTPrecondition precondition)
        {
            this.precondition = precondition;
        }
        /// <summary>
        /// ����ڵ�
        /// </summary>
        /// <param name="database"></param>
        public virtual void Activate(BTDataBase database)
        {
            if (activated)
            {
                return;
            }
            this.database = database;
            if (precondition != null)
                precondition.Activate(database);
            if (children != null)
                foreach (BTNode child in children)
                {
                    child.Activate(database);
                }
            activated = true;
        }

        public virtual void Clear()
        {

        }

        /// <summary>
        /// ���ڵ��ܷ�ִ�У������Ƿ�activated���Ƿ���ȴ��ɣ��Ƿ�ͨ��׼���������͸��Ի���� (DoEvaluate)
        /// </summary>
        /// <returns></returns>
        internal bool Evaluate()
        {
            bool coolDownOk = CheakTimer();
            return activated && coolDownOk && (precondition == null || precondition.Check()) && DoEvaluate();
        }

        protected virtual bool DoEvaluate()
        {
            return true;
        }

        protected bool CheakTimer()
        {
            if (Time.time - lastTimeEvaluated > interval)
            {
                lastTimeEvaluated = Time.time;
                return true;
            }
            return false;
        }

        public virtual BTResult Tick()
        {
            return BTResult.Ended;
        }

        public virtual void AddChild(BTNode node)
        {
            if (children == null)
            {
                children = new List<BTNode>();
            }
            if (node != null)
            {
                children.Add(node);
            }

        }

        public virtual void RemoveChild(BTNode node)
        {
            if (children != null && node != null)
                children.Remove(node);
        }
    }
    /// <summary>
    /// �ڵ������״̬
    /// </summary>
    public enum BTResult
    {
        /// <summary>
        /// �ɹ�
        /// </summary>
        Ended = 1,
        /// <summary>
        /// ������
        /// </summary>
        Running = 2,
        /// <summary>
        /// ʧ��
        /// </summary>
        Fail = 3,
    }
}
