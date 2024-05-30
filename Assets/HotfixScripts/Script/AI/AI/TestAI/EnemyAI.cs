using BT;
using UnityEngine;
using UnityEngine.Timeline;

public class EnemyAI : BTTree
{
    public CharacterActor actor;
    public Enemy enemy;
    EnemyAIControl control;
    public EnemyAI(EnemyAIControl control)
    {
        this.control = control;
    }
    public void Init(Enemy enemy, DataBase dataBase = null)
    {
        this.enemy = enemy;
        dataBase.SetData("actor", actor);
        dataBase.SetData("control", control);
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
        //var moveNode = new BTParallel(ParallelType.Wait);

        rootNode.AddChild(randomNode);
        rootNode.AddChild(new BTMoveAction("移动", 2));
        //rootNode.AddChild(new BTFindEnemy("寻找敌人", "Enemy") { setDataName = "target" });
        rootNode.AddChild(new BTSkillAction(control.skillRunner, Resources.Load<TimelineAsset>("test"), "combo1"));


        //rootNode.AddChild(rootNode);

        root = rootNode;
    }
}
