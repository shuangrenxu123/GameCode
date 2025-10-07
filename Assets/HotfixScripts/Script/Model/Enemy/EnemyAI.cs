using BT;
using UnityEngine;

/// <summary>
/// 敌人AI数据库键值枚举：定义行为树数据库中使用的键值
/// </summary>
public enum EnemyAIDatabaseKey
{
    /// <summary>
    /// 角色角色控制器
    /// </summary>
    CharacterActor,

    /// <summary>
    /// 战斗实体
    /// </summary>
    CombatEntity,

    /// <summary>
    /// 变换组件
    /// </summary>
    Transform,

    /// <summary>
    /// 巡逻半径
    /// </summary>
    PatrolRadius,

    /// <summary>
    /// 检测范围
    /// </summary>
    DetectionRange,

    /// <summary>
    /// 攻击范围
    /// </summary>
    AttackRange,

    /// <summary>
    /// 移动速度
    /// </summary>
    MoveSpeed,

    /// <summary>
    /// 目标位置
    /// </summary>
    Target,

    /// <summary>
    /// 目标是否在范围内
    /// </summary>
    TargetInRange,

    /// <summary>
    /// 是否可以攻击
    /// </summary>
    CanAttack,

    /// <summary>
    /// 敌人身体引用
    /// </summary>
    EnemyBody
}

/// <summary>
/// 敌人AI行为树大脑：使用行为树实现基本的巡逻和追击行为
/// </summary>
public class EnemyBT<TKey, TValue> : BTTree<TKey, TValue>
{
    /// <summary>
    /// 初始化行为树
    /// </summary>
    /// <param name="database">行为树数据库</param>
    public void Init(DataBase<TKey, TValue> database)
    {
        base.Init(database);
    }

    /// <summary>
    /// 设置行为树节点结构
    /// </summary>
    public override void SetNode()
    {
        var rootNode = new BTSelector<TKey, TValue>();

        // 巡逻行为序列
        var patrolSequence = new BTSequence<TKey, TValue>();
        patrolSequence.AddChild(new BTSetRandomPatrolPoint<TKey, TValue>());
        patrolSequence.AddChild(new BTMoveToTarget<TKey, TValue>());
        patrolSequence.AddChild(new BTWait<TKey, TValue>(3f));

        // 追击行为序列
        var chaseSequence = new BTSequence<TKey, TValue>();
        chaseSequence.AddChild(new BTCheckTargetInRange<TKey, TValue>());
        chaseSequence.AddChild(new BTMoveToTarget<TKey, TValue>());
        chaseSequence.AddChild(new BTCheckCanAttack<TKey, TValue>());
        chaseSequence.AddChild(new BTAttack<TKey, TValue>());

        rootNode.AddChild(chaseSequence);
        rootNode.AddChild(patrolSequence);

        root = rootNode;
    }
}

/// <summary>
/// 设置随机巡逻目标点行为节点
/// </summary>
public class BTSetRandomPatrolPoint<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var transform = database.GetData<Transform>((TKey)(object)EnemyAIDatabaseKey.Transform);
        var patrolRadius = database.GetData<float>((TKey)(object)EnemyAIDatabaseKey.PatrolRadius);

        if (transform == null)
            return BTResult.Failed;

        // 生成随机巡逻点
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * patrolRadius;
        Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        database.SetData((TKey)(object)EnemyAIDatabaseKey.Target, randomPoint);
        return BTResult.Success;
    }
}

/// <summary>
/// 检查目标是否在范围内行为节点
/// </summary>
public class BTCheckTargetInRange<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var characterActor = database.GetData<CharacterActor>((TKey)(object)EnemyAIDatabaseKey.CharacterActor);
        var target = database.GetData<Vector3>((TKey)(object)EnemyAIDatabaseKey.Target);
        var detectionRange = database.GetData<float>((TKey)(object)EnemyAIDatabaseKey.DetectionRange);

        if (characterActor == null)
            return BTResult.Failed;

        float distance = Vector3.Distance(characterActor.Position, target);
        database.SetData((TKey)(object)EnemyAIDatabaseKey.TargetInRange, distance <= detectionRange);

        return distance <= detectionRange ? BTResult.Success : BTResult.Failed;
    }
}

/// <summary>
/// 移动到目标位置行为节点
/// </summary>
public class BTMoveToTarget<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var characterActor = database.GetData<CharacterActor>((TKey)(object)EnemyAIDatabaseKey.CharacterActor);
        var target = database.GetData<Vector3>((TKey)(object)EnemyAIDatabaseKey.Target);
        var moveSpeed = database.GetData<float>((TKey)(object)EnemyAIDatabaseKey.MoveSpeed);

        if (characterActor == null)
            return BTResult.Failed;

        // 使用CharacterActor进行移动
        Vector3 direction = (target - characterActor.Position).normalized;
        characterActor.Velocity = direction * moveSpeed;

        // 检查是否到达目标
        float distance = Vector3.Distance(characterActor.Position, target);
        if (distance < 1f)
            return BTResult.Success;

        return BTResult.Running;
    }
}

/// <summary>
/// 检查是否可以攻击行为节点
/// </summary>
public class BTCheckCanAttack<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var characterActor = database.GetData<CharacterActor>((TKey)(object)EnemyAIDatabaseKey.CharacterActor);
        var target = database.GetData<Vector3>((TKey)(object)EnemyAIDatabaseKey.Target);
        var attackRange = database.GetData<float>((TKey)(object)EnemyAIDatabaseKey.AttackRange);

        if (characterActor == null)
            return BTResult.Failed;

        float distance = Vector3.Distance(characterActor.Position, target);
        database.SetData((TKey)(object)EnemyAIDatabaseKey.CanAttack, distance <= attackRange);

        return distance <= attackRange ? BTResult.Success : BTResult.Failed;
    }
}

/// <summary>
/// 攻击行为节点
/// </summary>
public class BTAttack<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var canAttack = database.GetData<bool>((TKey)(object)EnemyAIDatabaseKey.CanAttack);

        if (!canAttack)
            return BTResult.Failed;

        // 这里可以实现具体的攻击逻辑
        // 例如：播放攻击动画、计算伤害等
        Debug.Log("敌人发动攻击！");

        return BTResult.Success;
    }
}

/// <summary>
/// 等待行为节点
/// </summary>
public class BTWait<TKey, TValue> : BTAction<TKey, TValue>
{
    private float waitTime;
    private float timer = 0f;

    public BTWait(float time)
    {
        waitTime = time;
    }

    protected override BTResult Execute()
    {
        timer += Time.deltaTime;
        if (timer >= waitTime)
        {
            timer = 0f;
            return BTResult.Success;
        }

        return BTResult.Running;
    }
}