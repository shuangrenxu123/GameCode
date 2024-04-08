using BT;
using UnityEngine;

public class EnemyAI : BTTree
{
    public CharacterActor actor;
    public Enemy enemy;
    public void Init(Enemy enemy, DataBase dataBase = null)
    {
        this.enemy = enemy;
        dataBase.SetData("actor", actor);
        base.Init(dataBase);
    }
    public override void SetNode()
    {
        var rootNode = new BTSequence();


        var randomNode = new BTRandomTargetPosition("寻找随机巡逻点")
        {
            Range = new Vector2(10, 10),
            setDataName = "target"
        };
        var moveNode = new BTParallel(ParallelType.Wait);

        moveNode.AddChild(new BTMoveAction("移动",3));
        moveNode.AddChild(new BTFindEnemy("寻找敌人", "Enemy") { setDataName = "target" });

        rootNode.AddChild(randomNode);
        rootNode.AddChild(moveNode);

        root = rootNode;
    }
}
