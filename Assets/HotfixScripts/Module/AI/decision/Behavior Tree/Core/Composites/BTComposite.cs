using System.Collections.Generic;

namespace BT
{
    /// <summary>
    /// 组合节点，用于控制叶子节点怎么执行
    /// </summary>
    public class BTComposite<TKey, TValue> : BTNode<TKey, TValue>
    {
        private List<BTNode<TKey, TValue>> _children;
        public List<BTNode<TKey, TValue>> children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<BTNode<TKey, TValue>>();
                }
                return _children;
            }
        }
        public override void Activate(DataBase<TKey, TValue> database)
        {
            base.Activate(database);

            foreach (BTNode<TKey, TValue> child in children)
            {
                child.Activate(database);
            }
        }

        public virtual BTComposite<TKey, TValue> AddChild(BTNode<TKey, TValue> node)
        {
            if (node != null)
            {
                children.Add(node);
            }
            return this;
        }

        public virtual void RemoveChild(BTNode<TKey, TValue> node)
        {
            children.Remove(node);
        }
        public override void Clear()
        {
            base.Clear();
            foreach (BTNode<TKey, TValue> child in children)
            {
                child.Clear();
            }
        }
    }
}