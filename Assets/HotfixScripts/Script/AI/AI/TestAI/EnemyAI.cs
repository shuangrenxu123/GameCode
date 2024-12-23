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

        rootNode.AddChild(randomNode);
        rootNode.AddChild(new BTMoveAction("移动", 2));
        var randomSelectNode = new BTRandom();
        randomSelectNode.AddChild(50, new BTSkillAction(control.skillRunner, Resources.Load<TimelineAsset>("BuffTest"), "buffTest"));
        randomSelectNode.AddChild(50, new BTSkillAction(control.skillRunner, Resources.Load<TimelineAsset>("test"), "test"));
        randomSelectNode.AddChild(50, new BTSkillAction(control.skillRunner, Resources.Load<TimelineAsset>("test2"), "test2"));
        randomSelectNode.AddChild(50, new BTSkillAction(control.skillRunner, Resources.Load<TimelineAsset>("step"), "step"));
        var node = new BTProbability(50, randomSelectNode);
        rootNode.AddChild(node);

        root = rootNode;
    }
}
