using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : BTTree
{
    public override void SetNode()
    {
        var rootNode = new BTSequence();
        rootNode.AddChild(new BTRandomTargetPosition("targetPosition",transform));

        var rootP = new BTParallel(ParallelType.And);
        rootP.AddChild(new BTFindEnemy("targetPosition","Enemy",transform));
        rootP.AddChild(new BTMoveAction(transform, 3, "targetPosition", 2));

        rootNode.AddChild(rootP);
        root = rootNode;
    }
}
