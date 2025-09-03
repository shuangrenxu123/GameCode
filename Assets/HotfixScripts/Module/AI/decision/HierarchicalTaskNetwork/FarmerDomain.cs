using System;
using System.Collections.Generic;
using UnityEngine;
namespace HTN
{
    /// <summary>
    /// 农夫条件：体力是否足够的条件检查
    /// </summary>
    public class StaminaCondition : HTNCondition
    {
        private int minStamina;

        public StaminaCondition(int minStamina)
        {
            this.minStamina = minStamina;
        }

        public override bool Check(WorldState ws)
        {
            int currentStamina = ws.GetInt(WSProperties.WS_FarmerStamina);
            bool result = currentStamina >= minStamina;


            return result;
        }
    }

    /// <summary>
    /// 有可用斧头的条件检查（耐久度>0）
    /// </summary>
    public class HasAxeCondition : HTNCondition
    {
        public override bool Check(WorldState ws)
        {
            return ws.GetInt(WSProperties.WS_AxeDurability) > 0;
        }
    }

    /// <summary>
    /// 只能徒手工作的条件检查（斧头损坏）
    /// </summary>
    public class NoAxeCondition : HTNCondition
    {
        public override bool Check(WorldState ws)
        {
            return ws.GetInt(WSProperties.WS_AxeDurability) <= 0;
        }
    }

    /// <summary>
    /// 有足够木材能够修斧头的条件检查
    /// </summary>
    public class HasWoodForRepairCondition : HTNCondition
    {
        public override bool Check(WorldState ws)
        {
            return ws.GetInt(WSProperties.WS_WoodCount) > 0;
        }
    }

    /// <summary>
    /// 用斧子砍柴任务：使用斧头砍柴，产量随耐久度变化，消耗工具耐久
    /// </summary>
    public class AxeChopTask : PrimitiveTask
    {
        public AxeChopTask(DomainBase domain) :
            base(domain, "AxeChop", TaskType.Primitive, new List<HTNCondition> { new StaminaCondition(10), new HasAxeCondition() })
        {
            ApplyEffects = (worldState) =>
            {
                int currentStamina = worldState.GetInt(WSProperties.WS_FarmerStamina);
                int currentWood = worldState.GetInt(WSProperties.WS_WoodCount);
                int currentDurability = worldState.GetInt(WSProperties.WS_AxeDurability);

                // 根据斧头耐久度决定砍柴产量
                int woodGained = currentDurability > 5 ? 3 : 2;

                // 使用斧头砍柴
                worldState.Set(WSProperties.WS_FarmerStamina, currentStamina - 10);
                worldState.Set(WSProperties.WS_WoodCount, currentWood + woodGained);
                worldState.Set(WSProperties.WS_AxeDurability, currentDurability - 1);
            };
        }

        public override HTNResults Execute()
        {
            return HTNResults.succeed;
        }
    }

    /// <summary>
    /// 徒手砍柴任务：斧头损坏时使用，低效但不消耗工具
    /// </summary>
    public class HandChopTask : PrimitiveTask
    {
        public HandChopTask(DomainBase domain) :
            base(domain, "HandChop", TaskType.Primitive, new List<HTNCondition> { new StaminaCondition(20), new NoAxeCondition() })
        {
            ApplyEffects = (worldState) =>
            {
                int currentStamina = worldState.GetInt(WSProperties.WS_FarmerStamina);
                int currentWood = worldState.GetInt(WSProperties.WS_WoodCount);

                // 徒手砍柴，低效但安全
                worldState.Set(WSProperties.WS_FarmerStamina, currentStamina - 20);
                worldState.Set(WSProperties.WS_WoodCount, currentWood + 1);
            };
        }

        public override HTNResults Execute()
        {
            return HTNResults.succeed;
        }
    }

    /// <summary>
    /// 修斧头任务：消耗1木材和1体力，恢复斧头耐久到10（可以修复完全损坏的斧头）
    /// </summary>
    public class RepairAxeTask : PrimitiveTask
    {
        public RepairAxeTask(DomainBase domain) :
            base(domain, "RepairAxe", TaskType.Primitive, new List<HTNCondition> {
                new StaminaCondition(5), new HasWoodForRepairCondition() })
        {
            ApplyEffects = (worldState) =>
            {
                int currentStamina = worldState.GetInt(WSProperties.WS_FarmerStamina);
                int currentWood = worldState.GetInt(WSProperties.WS_WoodCount);
                int currentDurability = worldState.GetInt(WSProperties.WS_AxeDurability);

                // 智能判断是否需要真正修理
                if (currentDurability < 10)
                {
                    // 需要修理的情况：消耗正常资源
                    worldState.Set(WSProperties.WS_FarmerStamina, currentStamina - 5);
                    worldState.Set(WSProperties.WS_WoodCount, currentWood - 1);
                    worldState.Set(WSProperties.WS_AxeDurability, 10);
                }
                else
                {
                    // 斧头状态良好：只消耗少量体力用于检查
                    worldState.Set(WSProperties.WS_FarmerStamina, currentStamina - 1); // 只消耗1体力用于检查
                    // 不消耗木材，不改变斧头耐久度
                    // 移除调试输出，保持代码简洁
                }
            };
        }

        public override HTNResults Execute()
        {
            return HTNResults.succeed;
        }
    }

    /// <summary>
    /// 休息任务
    /// </summary>
    public class RestTask : PrimitiveTask
    {
        public RestTask(DomainBase domain) :
            base(domain, "Rest", TaskType.Primitive)
        {
            ApplyEffects = (worldState) =>
            {
                int currentStamina = worldState.GetInt(WSProperties.WS_FarmerStamina);
                worldState.Set(WSProperties.WS_FarmerStamina, currentStamina + 15);
            };
        }

        public override HTNResults Execute()
        {
            return HTNResults.succeed;
        }
    }

    /// <summary>
    /// 农夫领域：包含完整的资源管理系统
    /// </summary>
    public class FarmerDomain : DomainBase
    {
        public FarmerDomain(WorldState ws) : base(ws) { }

        public override void Init()
        {
            // 创建原子任务 - 四个基础行为
            AxeChopTask axeChop = new AxeChopTask(this);            // ⚒️ 用斧子砍柴任务
            HandChopTask handChop = new HandChopTask(this);          // 🤏 徒手砍柴任务
            RepairAxeTask repairAxe = new RepairAxeTask(this);      // 🔧 修斧头任务
            RestTask rest = new RestTask(this);                     // 💤 休息任务

            // 🔥 🔥 🔥 创建核心复合任务：智能资源管理
            CompoundTask resourceManagementTask = new CompoundTask(this, "ResourceManagement", TaskType.Compound);

            // 🏆 方法1：最高效率策略 (体力≥80) - 智能砍柴模式：先快速积累木材，再立即修理斧头，然后继续活用最好的斧头砍柴
            Method highEfficiencyStrategy = new Method(1, new StaminaCondition(80));
            highEfficiencyStrategy.SubTasks.Add(axeChop);  // 第1次砍柴: +3木材, 耐久-1 (9/10)
            highEfficiencyStrategy.SubTasks.Add(axeChop);  // 第2次砍柴: +3木材, 耐久-1 (8/10)
            highEfficiencyStrategy.SubTasks.Add(repairAxe); // 修复斧头: 耐久恢复到10/10 (+3木材)
            highEfficiencyStrategy.SubTasks.Add(axeChop);  // 第3次砍柴: +3木材, 耐久-1 (9/10) - 斧头越新砍柴效率越高!
            highEfficiencyStrategy.SubTasks.Add(axeChop);  // 第4次砍柴: +3木材, 耐久-1 (8/10)
            highEfficiencyStrategy.SubTasks.Add(rest);     // 休息恢复: 确保有充足体力继续工作
            resourceManagementTask.AddMethod(highEfficiencyStrategy);

            // 🏄 方法2：均衡策略 (体力≥55) - 三次砍柴模式：合理利用初始斧头状态，维持可持续生产
            Method balancedStrategy = new Method(2, new StaminaCondition(55));
            balancedStrategy.SubTasks.Add(axeChop);    // 第1次砍柴: 根据初始耐久度获得木材
            balancedStrategy.SubTasks.Add(axeChop);    // 第2次砍柴: 继续积累
            balancedStrategy.SubTasks.Add(axeChop);    // 第3次砍柴: 保持生产节奏
            balancedStrategy.SubTasks.Add(rest);       // 休息恢复: 为下一轮做准备
            resourceManagementTask.AddMethod(balancedStrategy);

            // 🛠️ 方法3：实用策略 (体力≥35) - 修斧头后工作
            Method practicalStrategy = new Method(3, new StaminaCondition(35));
            practicalStrategy.SubTasks.Add(repairAxe); // 修理工具
            practicalStrategy.SubTasks.Add(axeChop);   // 用修理好的斧子工作
            practicalStrategy.SubTasks.Add(rest);      // 休息
            resourceManagementTask.AddMethod(practicalStrategy);

            // 📊 方法4：基础策略 (体力≥25) - 基础模式：徒手工作为主，尽可能的利用有限体力
            Method basicStrategy = new Method(4, new StaminaCondition(25));
            basicStrategy.SubTasks.Add(handChop);      // 利用体力砍柴
            basicStrategy.SubTasks.Add(rest);           // 恢复一部分体力
            basicStrategy.SubTasks.Add(handChop);      // 继续努力工作
            basicStrategy.SubTasks.Add(rest);           // 彻底休息
            resourceManagementTask.AddMethod(basicStrategy);

            // ⚠️ 方法5：生存策略 (体力≥15) - 最低限度维持：斧头损坏时的最后手段
            Method survivalStrategy = new Method(5, new StaminaCondition(15));
            survivalStrategy.SubTasks.Add(handChop);   // 勉强砍柴获取基本资源
            survivalStrategy.SubTasks.Add(rest);       // 充分休息恢复体力
            survivalStrategy.SubTasks.Add(handChop);   // 再次努力工作
            survivalStrategy.SubTasks.Add(rest);       // 彻底休整
            resourceManagementTask.AddMethod(survivalStrategy);

            // 将核心任务添加到HTN域
            AddTask(resourceManagementTask);
        }

        public override void BuildWorldState()
        {
            // 世界状态已在WorldState构造函数中初始化
        }
    }
}