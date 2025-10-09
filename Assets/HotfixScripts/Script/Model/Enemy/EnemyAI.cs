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
    EnemyBody
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

        var moveAction = new BTMoveAction();

        // var attackAction = new BTAttackNode();
        // var timerNode = new BTTimer(5f, attackAction);

        rootNode.AddChild(moveAction);

        root = rootNode;
    }
}

