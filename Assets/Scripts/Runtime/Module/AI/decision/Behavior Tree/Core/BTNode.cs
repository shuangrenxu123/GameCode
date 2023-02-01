using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// 所有节点的父节点
    /// </summary>
    public class BTNode
    {
        public string name;

        /// <summary>
        /// 子节点
        /// </summary>
        public List<BTNode> children;
        /// <summary>
        /// 检测是否可以进入此节点
        /// </summary>
        public BTPrecondition precondition;

        public BTDataBase database;

        /// <summary>
        /// 冷却时间
        /// </summary>
        public float interval = 0;
        private float lastTimeEvaluated = 0;
        /// <summary>
        /// 是否已经激活
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
        /// 激活节点
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
        /// 检查节点能否执行，包括是否activated，是否冷却完成，是否通过准入条件，和个性化检查 (DoEvaluate)
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
    /// 节点的运行状态
    /// </summary>
    public enum BTResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Ended = 1,
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 2,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = 3,
    }
}
