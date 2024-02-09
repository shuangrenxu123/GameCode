using BT;
using UnityEngine;

public class EnemyAI : BTTree
{
    Transform transform;
    public Enemy enemy;
    public void Init(Enemy enemy, Enemy e, DataBase dataBase = null)
    {
        this.enemy = enemy;
        Init(enemy, e, database);
    }
    public override void SetNode()
    {
        var rootNode = new BTParallel(ParallelType.Wait);
        var MainNoed = new BTSelector();
        #region main node
        MainNoed.AddChild(new BTSequence()
        .AddChild(new BTFindEnemy("追击节点下的寻找敌人", "targetTransform", "Controller", transform))

        .AddChild(new BTSelector()
            .AddChild(new BTSequence()
                .AddChild(new BTStrafing("围绕", enemy))
                .AddChild(new BTParallel(ParallelType.Or)
                    .AddChild(new BTMoveAction("追击敌人移动节点", transform, 1f, "targetTransform", 16))
                    )
                )
        ));

        #endregion
        rootNode.AddChild(MainNoed);
        root = rootNode;
    }
}
