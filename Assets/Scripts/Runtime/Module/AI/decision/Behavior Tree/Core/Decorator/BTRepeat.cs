using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BT
{
    public class BTRepeat : BTDecorator
    {
        public BTRepeat(BTNode child) : base(child)
        {

        }
        public override BTResult Tick()
        {
            var result = child.Tick();
            if(result == BTResult.Success)
            {
                return BTResult.Success;

            }
            if(result == BTResult.Failed)
            {
                child.Clear();
            }
            return BTResult.Running;
        }
    }
}