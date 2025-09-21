using BT;
using UnityEngine;
using UnityEngine.Timeline;

public class EnemyAI<TKey, TValue> : BTTree<TKey, TValue>
{
    public CharacterActor actor;
    public Enemy enemy;
    EnemyAIControl control;
    public EnemyAI(EnemyAIControl control)
    {
        this.control = control;
    }
    public void Init(Enemy enemy, DataBase<TKey, TValue> dataBase = null)
    {
        this.enemy = enemy;
        dataBase.SetData((dynamic)"actor", actor);
        dataBase.SetData((dynamic)"control", control);
        base.Init(dataBase);
    }
    public override void SetNode()
    {
        var rootNode = new BTSequence<TKey, TValue>();
        var randomNode = new BTRandomTargetPosition<TKey, TValue>()
        {
            Range = new Vector2(10, 10),
            setDataName = (dynamic)"target"
        };

        rootNode.AddChild(randomNode);
        rootNode.AddChild(new BTMoveAction<TKey, TValue>("移动", 2));
        var randomSelectNode = new BTRandom<TKey, TValue>();
        // Note: BTSkillAction needs to be made generic too
        // randomSelectNode.AddChild(50, new BTSkillAction<TKey, TValue>(control.skillRunner, Resources.Load<TimelineAsset>("BuffTest")));
        var node = new BTProbability<TKey, TValue>(50, randomSelectNode);
        rootNode.AddChild(node);

        root = rootNode;
    }
}
