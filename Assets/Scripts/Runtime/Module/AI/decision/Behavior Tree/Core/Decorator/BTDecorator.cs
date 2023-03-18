using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public class BTDecorator : BTNode
    {
        public BTNode child;
        public BTDecorator(BTNode child)
        {
            this.child = child;
        }
        public override void Activate(BTDataBase database)
        {
            base.Activate(database);
            child.Activate(database);
        }
        public override void Clear()
        {
            base.Clear();
            child.Clear();
        }
    }
}