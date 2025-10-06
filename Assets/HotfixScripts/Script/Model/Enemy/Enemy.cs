using System;
using Fight;
using UnityEngine;
using UnityEngine.Events;
using static Fight.Number.CombatNumberBox;

public class Enemy : MonoBehaviour
{
    [Header("身体组件")]
    public CharacterActor characterActor;    // 移动器官
    public CombatEntity combatEntity;        // 战斗器官

    [Header("AI系统")]
    [SerializeField] private string aiType = "Simple"; // AI类型标识
    // 暂时移除大脑引用，先实现基本功能

    [Header("巡逻设置")]
    [SerializeField] private Vector3 patrolCenter;      // 巡逻中心点
    [SerializeField] private float patrolRadius = 10f;  // 巡逻半径

    private void Awake()
    {
        // 初始化身体组件
        characterActor = GetComponent<CharacterActor>();
        combatEntity = GetComponent<CombatEntity>();
    }

    private void Start()
    {
        // 初始化战斗属性
        combatEntity.hp.SetMaxValue(100);
        combatEntity.properties.RegisterAttribute(PropertyType.Attack, 10);
        combatEntity.properties.RegisterAttribute(PropertyType.Defense, 10);
        combatEntity.properties.RegisterAttribute(PropertyType.SpeedMultiplier, 10);

        // 初始化完成
    }

    /// <summary>
    /// 设置巡逻中心点（从当前位置开始巡逻）
    /// </summary>
    public void SetPatrolCenter(Vector3 center)
    {
        patrolCenter = center;
    }

    /// <summary>
    /// 设置巡逻半径
    /// </summary>
    public void SetPatrolRadius(float radius)
    {
        patrolRadius = radius;
    }

    private void Update()
    {
        // 简单的AI逻辑：暂时只做基础移动
        SimpleAIThink();
    }

    /// <summary>
    /// 简单的AI思考逻辑（临时实现）
    /// </summary>
    private void SimpleAIThink()
    {
        // 这里暂时实现简单的AI逻辑
        // 以后可以用更复杂的行为树替换
        if (characterActor != null && characterActor.IsGrounded)
        {
            // 简单的随机移动
            Vector3 randomDirection = new Vector3(
                Mathf.Sin(Time.time) * 0.5f,
                0f,
                Mathf.Cos(Time.time) * 0.5f
            );
            characterActor.Velocity = randomDirection;
        }
    }

    /// <summary>
    /// 更换AI类型（预留接口）
    /// </summary>
    /// <param name="newAIType">新的AI类型</param>
    public void ChangeBrain(string newAIType)
    {
        aiType = newAIType;
        // 以后实现大脑切换逻辑
    }

    /// <summary>
    /// 获取当前AI类型
    /// </summary>
    public string CurrentAIType => aiType;

    /// <summary>
    /// 身体状态查询接口
    /// </summary>
    public Vector3 Position => characterActor != null ? characterActor.Position : transform.position;
    public bool IsGrounded => characterActor != null && characterActor.IsGrounded;
    public float Health => combatEntity != null ? combatEntity.hp.Value : 0;
    public bool IsDead => Health <= 0;

    private void OnDestroy()
    {
        // 清理AI资源
    }
}
