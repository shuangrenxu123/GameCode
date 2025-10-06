# 人类敌人AI架构

## 架构概述

本架构采用"大脑-身体"的设计模式，实现了Enemy和EnemyAI的完全解耦：

- **Enemy**：作为身体容器，只负责物理组件的管理
- **EnemyAI**：作为大脑，只负责决策和指令发送
- **行为树节点**：作为身体器官，负责具体动作的执行

## 文件结构

```
Assets/HotfixScripts/Script/Model/Enemy/
├── Enemy.cs              # 身体容器：整合CharacterActor和CombatEntity
├── IEnemyBrain.cs        # 大脑接口：定义决策行为协议
├── EnemyAIFactory.cs     # AI工厂：创建和管理不同类型的大脑
├── DefaultEnemyAI.cs     # 默认AI大脑：基础巡逻和追击行为
├── SimpleEnemyAI.cs      # 简化AI大脑：直接集成行为树
├── EnemyAI.cs            # 通用行为树大脑：可扩展的行为树实现
├── EnemyAISample.cs      # 使用示例：展示如何创建和配置敌人AI
└── README.md             # 本文档
```

## 核心类详解

### 1. Enemy.cs（身体容器）

**主要功能**：
- 整合CharacterActor（移动器官）和CombatEntity（战斗器官）
- 管理身体的生命周期和状态
- 提供大脑装配接口

**关键接口**：
```csharp
public void ChangeBrain(string newAIType)  // 运行时切换AI类型
public string CurrentAIType { get; }       // 获取当前AI类型
public Vector3 Position { get; }           // 获取当前位置
public bool IsGrounded { get; }            // 获取地面状态
public float Health { get; }               // 获取生命值
public bool IsDead { get; }                // 检查是否死亡
```

### 2. IEnemyBrain.cs（大脑接口）

**设计目的**：
- 定义大脑与身体的通信协议
- 实现不同AI类型的可插拔支持

**接口方法**：
```csharp
void Initialize(Enemy body)    // 初始化大脑，注入身体引用
void Think()                   // 思考过程（每帧调用）
void Shutdown()                // 关闭大脑
```

### 3. EnemyAIFactory.cs（AI工厂）

**主要功能**：
- 管理不同类型AI的注册和创建
- 支持运行时AI类型的动态切换

**核心方法**：
```csharp
public static IEnemyBrain CreateBrain(string aiType, Enemy body)
public static void RegisterBrainType(string name, Type brainType)
```

### 4. DefaultEnemyAI.cs（默认AI大脑）

**行为特点**：
- 使用行为树实现决策
- 支持巡逻、追击、攻击等基础行为
- 通过DataBase与身体通信

## 使用方法

### 基本使用

1. **创建敌人**：
```csharp
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public void SpawnEnemy()
    {
        var enemy = Instantiate(enemyPrefab);
        var enemyComponent = enemy.GetComponent<Enemy>();

        // 设置巡逻参数
        enemyComponent.SetPatrolCenter(enemy.transform.position);
        enemyComponent.SetPatrolRadius(10f);
    }
}
```

2. **配置敌人属性**：
```csharp
// 在Inspector中设置AI类型
public class EnemyAuthoring : MonoBehaviour
{
    public string aiType = "Default";  // 可选值：Default, Aggressive, Cowardly等
    public float patrolRadius = 10f;
}
```

### 高级用法

3. **运行时切换AI**：
```csharp
public class AIDebugger : MonoBehaviour
{
    public void MakeEnemyAggressive(Enemy enemy)
    {
        enemy.ChangeBrain("Aggressive");
    }

    public void MakeEnemyCowardly(Enemy enemy)
    {
        enemy.ChangeBrain("Cowardly");
    }
}
```

4. **自定义AI类型**：
```csharp
// 1. 实现IEnemyBrain接口
public class CustomEnemyAI : IEnemyBrain
{
    public void Initialize(Enemy body) { /* 初始化逻辑 */ }
    public void Think() { /* 决策逻辑 */ }
    public void Shutdown() { /* 清理逻辑 */ }
}

// 2. 注册到工厂
public class AIRegistrator : MonoBehaviour
{
    private void Awake()
    {
        EnemyAIFactory.RegisterBrainType("Custom", typeof(CustomEnemyAI));
    }
}
```

## 扩展指南

### 添加新的AI类型

1. **创建新的大脑类**：
```csharp
public class PatrolEnemyAI : IEnemyBrain
{
    private Enemy body;
    private float patrolTimer = 0f;

    public void Initialize(Enemy enemyBody)
    {
        body = enemyBody;
    }

    public void Think()
    {
        // 实现巡逻逻辑
        patrolTimer += Time.deltaTime;
        if (patrolTimer > 5f)
        {
            // 改变巡逻方向
            patrolTimer = 0f;
        }
    }

    public void Shutdown() { }
}
```

2. **注册AI类型**：
```csharp
EnemyAIFactory.RegisterBrainType("Patrol", typeof(PatrolEnemyAI));
```

### 添加新的行为树节点

1. **创建行为节点**：
```csharp
public class BTGuardPosition<TKey, TValue> : BTAction<TKey, TValue>
{
    protected override BTResult Execute()
    {
        var characterActor = database.GetData<CharacterActor>("characterActor");
        var guardPosition = database.GetData<Vector3>("guardPosition");

        // 实现守卫逻辑
        Vector3 direction = (guardPosition - characterActor.Position).normalized;
        characterActor.Velocity = direction * 3f;

        return BTResult.Running;
    }
}
```

## 设计优势

1. **完全解耦**：Enemy不依赖任何具体的AI实现
2. **灵活扩展**：支持运行时AI类型切换
3. **职责分离**：大脑专注决策，身体专注执行
4. **易于测试**：AI逻辑可以独立测试
5. **性能优化**：支持AI的启用/禁用

## 注意事项

1. **命名空间**：所有文件都在全局命名空间中，确保类型名称不冲突
2. **生命周期**：大脑的Initialize和Shutdown方法要正确实现资源管理
3. **数据共享**：通过DataBase进行大脑和身体之间的数据通信
4. **异常处理**：AI逻辑中要处理身体组件为null的情况

## 未来扩展

- 支持行为树的可视化编辑
- 添加AI状态机支持
- 实现群体AI行为
- 添加学习和适应机制