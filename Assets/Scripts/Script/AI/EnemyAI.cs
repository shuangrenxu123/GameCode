using BT;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : BTTree
{
    public EnemyAnimatorHandle animator;
    Transform transform;
    public NavMeshAgent agent;
    public override void Init(Enemy enemy, EnemyAnimatorHandle anim,Enemy e, BTDataBase dataBase = null)
    {
        this.enemy = enemy;
        transform = enemy.transform;
        animator = anim;
        base.Init(enemy,anim,e,database);
    }
    public override void SetNode()
    {
        var rootNode = new BTParallel(ParallelType.Wait);
        var MainNoed = new BTSelector();
        #region main node
        MainNoed.AddChild(new BTSequence()
        .AddChild(new BTFindEnemy("追击节点下的寻找敌人","targetTransform", "Controller", transform))

        .AddChild(new BTSelector()
            .AddChild(new BTSequence()
                .AddChild(new BTStrafing("围绕", animator))
                .AddChild(new BTParallel(ParallelType.Or) 
                    .AddChild(new BTMoveAction("追击敌人移动节点",transform, 1f, "targetTransform", 16))
                    .AddChild(new BTAnimatorAction("追击敌人跑步动画",animator, "Walk", true))
                    )

                .AddChild(new BTAnimatorAction("攻击动画", animator, "attack"))
                )
        ));

        #endregion
        rootNode.AddChild(MainNoed);
        root = rootNode;
    }
}
