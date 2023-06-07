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
        public override void Activate(BTDataBase database, Enemy e)
        {
            base.Activate(database, e);
            child.Activate(database, e);
        }
        public override void Clear()
        {
            base.Clear();
            child.Clear();
        }
    }
}