using System.Collections.Generic;
using AIBlackboard;

namespace BT
{
    /// <summary>
    /// 组合节点，用于控制叶子节点怎么执行
    /// </summary>
    public class BTComposite : BTNode
    {
        private List<BTNode> _children;
        public List<BTNode> children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<BTNode>();
                }
                return _children;
            }
        }
        public override void Activate(Blackboard database)
        {
            base.Activate(database);

            foreach (BTNode child in children)
            {
                child.Activate(database);
            }
        }

        public virtual BTComposite AddChild(BTNode node)
        {
            if (node != null)
            {
                children.Add(node);
            }
            return this;
        }

        public virtual void RemoveChild(BTNode node)
        {
            children.Remove(node);
        }
        public override void Clear()
        {
            base.Clear();
            foreach (BTNode child in children)
            {
                child.Clear();
            }
        }
    }
}