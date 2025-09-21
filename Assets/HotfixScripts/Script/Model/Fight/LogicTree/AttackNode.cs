using System.Collections.Generic;
using BT;
using UnityEngine;
namespace Fight
{
    public class AttackNode : BTAction<string, object>
    {
        protected override BTResult Execute()
        {
            return BTResult.Success;
        }
    }
}
