using BT;
using UnityEngine;

public class EnemyAI : BTTree
{
    public Animator animator;
    public SkillSystem skillSystem;
    public override void SetNode()
    {
        var rootNode = new BTParallel(ParallelType.Wait);

        var MainNoed = new BTSelector();

        #region main node

        MainNoed.AddChild(new BTSequence()
        .AddChild(new BTFindEnemy("追击节点下的寻找敌人","targetTransform", "Enemy", transform))

        .AddChild(new BTSelector()
            .AddChild(new BTinterval(databaseName: "jili",5,new BTAnimatorAction("释放buff动画",animator,"jili")))
            .AddChild(new BTSequence()
                .AddChild(new BTParallel(ParallelType.Or)
                    .AddChild(new BTMoveAction("追击敌人移动节点",transform, 3.5f, "targetTransform", 2))
                    .AddChild(new BTAnimatorAction("追击敌人跑步动画",animator, "run", true))
                    )
                .AddChild(new BTAnimatorAction("攻击动画", animator, "attack"))
                )
        ));

        MainNoed.AddChild(new BTSequence()
            .AddChild(new BTRandomTargetPosition("随机设置移动点", "targetTransform", transform))
        
            .AddChild(new BTParallel(ParallelType.Or)
                .AddChild(new BTMoveAction("巡逻移动节点", transform, 2, "targetTransform", 2))
                .AddChild(new BTAnimatorAction("行走动画", animator, "walk", true))
                .AddChild(new BTRepeat(new BTFindEnemy("巡逻节点下的寻找敌人", "targetTransform", "Enemy", transform))))
        
            .AddChild(new BTParallel(ParallelType.Or)
                .AddChild(new BTAnimatorAction("站立动画", animator, "idle"))
                .AddChild(new BTRepeat(new BTFindEnemy("站立节点下的寻找敌人", "targetTransform", "Enemy", transform))))
            );


        #endregion
        //分支的节点负责检测外部事件
        rootNode.AddChild(MainNoed)
            .AddChild(new BTUpdateTime("更新数据库").AddTimer("jili"));
        root = rootNode;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,10);
    }
}
