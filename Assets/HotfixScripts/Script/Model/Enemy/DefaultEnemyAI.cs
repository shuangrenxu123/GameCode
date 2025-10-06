using BT;
using UnityEngine;

/// <summary>
/// 默认敌人AI大脑：使用行为树实现基本的巡逻和追击行为
/// </summary>
public class DefaultEnemyAI : IEnemyBrain
{
    private Enemy body;
    private EnemyBT<string, object> behaviorTree;

    /// <summary>
    /// 初始化大脑，注入身体引用
    /// </summary>
    /// <param name="enemyBody">敌人的身体组件</param>
    public void Initialize(Enemy enemyBody)
    {
        body = enemyBody;

        // 初始化行为树数据库
        var database = new DataBase<string, object>();
        database.SetData("characterActor", body.characterActor);
        database.SetData("combatEntity", body.combatEntity);
        database.SetData("transform", body.transform);
        database.SetData("detectionRange", 15f);
        database.SetData("attackRange", 3f);
        database.SetData("moveSpeed", 5f);

        // 创建行为树大脑
        behaviorTree = new EnemyBT<string, object>();
        behaviorTree.Init(database);
    }

    /// <summary>
    /// 思考过程（每帧调用）
    /// </summary>
    public void Think()
    {
        behaviorTree.Update();
    }

    /// <summary>
    /// 关闭大脑
    /// </summary>
    public void Shutdown()
    {
        // 清理资源
        behaviorTree = null;
    }
}