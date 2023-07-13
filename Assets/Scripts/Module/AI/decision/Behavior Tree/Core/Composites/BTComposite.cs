using System.Collections.Generic;

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
        public override void Activate(DataBase database, Enemy e)
        {
            base.Activate(database, e);

            foreach (BTNode child in children)
            {
                child.Activate(database, e);
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
    }
}