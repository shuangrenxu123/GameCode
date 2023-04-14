using BT;
using UnityEngine;

public class EnemyAI : BTTree
{
    public Animator animator;
    public override void SetNode()
    {
        var rootNode = new BTParallel(ParallelType.Or);

        var MainNoed = new BTSelector();
        MainNoed.AddChild(new BTSequence()
           .AddChild(new BTFindEnemy("targetTransform", "Enemy", transform))
           .AddChild(new BTParallel(ParallelType.Or)
               .AddChild(new BTMoveAction(transform, 3.5f, "targetTransform", 2))
               .AddChild(new BTAnimatorAction(animator, "run", true)))
           .AddChild(new BTAnimatorAction(animator,"attack"))
           );

        MainNoed.AddChild(new BTSequence()
            .AddChild(new BTRandomTargetPosition("targetTransform", transform))
            .AddChild(new BTParallel(ParallelType.Or)
                .AddChild(new BTMoveAction(transform, 2, "targetTransform", 2))
                .AddChild(new BTAnimatorAction(animator, "walk", true))
                .AddChild(new BTRepeat(new BTFindEnemy("targetTransform", "Enemy", transform))))
            .AddChild(new BTParallel(ParallelType.Or)
                .AddChild(new BTAnimatorAction(animator, "idle"))
                .AddChild(new BTRepeat(new BTFindEnemy("targetTransform", "Enemy", transform))))
            );
        //分支的节点负责检测外部事件
        rootNode.AddChild(MainNoed).AddChild(new BTFall());
        root = rootNode;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,10);
    }
}
