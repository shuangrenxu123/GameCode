using BT;
using BT.Action;
using CharacterController;
using Fight;
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
    EnemyBody,

    /// <summary>
    /// 效用AI大脑引用
    /// </summary>
    UtilityBrain,

    /// <summary>
    /// 巡逻起点
    /// </summary>
    PatrolOrigin,

    /// <summary>
    /// 逃跑的安全距离
    /// </summary>
    FleeSafeDistance,

    /// <summary>
    /// 低血量阈值
    /// </summary>
    LowHealthThreshold,

    /// <summary>
    /// 当前与目标距离
    /// </summary>
    PlayerDistance,

    /// <summary>
    /// 目标是否在感知范围内
    /// </summary>
    PlayerVisible,

    /// <summary>
    /// 巡逻目的地
    /// </summary>
    PatrolDestination,

    /// <summary>
    /// 效用AI决策结果
    /// </summary>
    UtilityDecision,

    /// <summary>
    /// 巡逻速度倍率
    /// </summary>
    PatrolSpeedMultiplier,

    /// <summary>
    /// 受击翻滚概率
    /// </summary>
    RollChanceOnHit,

    /// <summary>
    /// 是否应该翻滚（受击判定后写入）
    /// </summary>
    ShouldRoll,

    /// <summary>
    /// 最后一次受到伤害的来源
    /// </summary>
    LastDamager
}

/// <summary>
/// 敌人AI行为树大脑：使用行为树实现基本的巡逻和追击行为
/// </summary>
public class EnemyBT : BTTree
{
    /// <summary>
    /// 设置行为树节点结构
    /// </summary>
    public override void SetNode()
    {
        var rootNode = new BTSequence();

        // 1. Update Utility Brain
        var utilityNode = new BTUtilityUpdateNode();
        rootNode.AddChild(utilityNode);

        // 2. Selector to choose behavior based on decision
        var selector = new BTSelector();
        rootNode.AddChild(selector);

        // 2.1 Patrol Branch
        var patrolSequence = new BTSequence();
        patrolSequence.AddChild(new BTCheckUtilityDecisionNode("Patrol"));
        patrolSequence.AddChild(new BTPatrolNode());
        selector.AddChild(patrolSequence);

        // 2.2 Flee Branch
        var fleeSequence = new BTSequence();
        fleeSequence.AddChild(new BTCheckUtilityDecisionNode("Flee"));
        fleeSequence.AddChild(new BTFleeNode());
        selector.AddChild(fleeSequence);

        // 2.3 Chase Branch
        var chaseSequence = new BTSequence();
        chaseSequence.AddChild(new BTCheckUtilityDecisionNode("Chase"));
        chaseSequence.AddChild(new BTChaseNode());
        selector.AddChild(chaseSequence);

        // 2.4 Attack Branch
        var attackSequence = new BTSequence();
        attackSequence.AddChild(new BTCheckUtilityDecisionNode("Attack"));
        attackSequence.AddChild(new BTAttackNode());
        selector.AddChild(attackSequence);

        root = rootNode;
    }
}

