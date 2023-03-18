using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// 所有修饰器节点的父类，他有且仅有一个子节点，常用于对节点返回值的修饰
    /// </summary>
    public class BTComposite : BTNode
    {
        private List<BTNode> _children;
        private List<BTNode> _selectedChildrenForClear;

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
        protected List<BTNode> selectedChildrenForClear
        {
            get
            {
                if (_selectedChildrenForClear == null)
                {
                    _selectedChildrenForClear = new List<BTNode>();
                }
                return _selectedChildrenForClear;
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

        public void AddChild(BTNode node)
        {
            if (node != null)
            {
                children.Add(node);
            }
        }

        public void AddChild(BTNode node, bool selectForClear)
        {
            AddChild(node);
            if (selectForClear)
            {
                selectedChildrenForClear.Add(node);
            }
        }

        public void RemoveChild(BTNode node)
        {
            children.Remove(node);
            selectedChildrenForClear.Remove(node);
        }
    }
}