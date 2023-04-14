using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// 
    /// </summary>
    public class BTComposite : BTNode
    {
        private List<BTNode> _children;
        public BTClearOpt clearOpt { get; set; }
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
        public override void Activate(BTDataBase database)
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
    }
}