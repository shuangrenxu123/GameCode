using System;
using System.Collections.Generic;
namespace HTN
{
    public enum HTNResults
    {
        /// <summary>
        /// 执行成功
        /// </summary>
        succeed,
        /// <summary>
        /// 执行失败
        /// </summary>
        fail,
    }
    public abstract class PrimitiveTask : TaskBase
    {
        public PrimitiveTask(DomainBase domain, string name, TaskType t, List<HTNCondition> c = null) : base(domain,name, t, c)
        {

        }
        /// <summary>
        /// 执行后会对世界造成的影响
        /// </summary>
        public Action<WorldState> ApplyEffects;
        /// <summary>
        /// 期望影响（只在规划、检查阶段对世界状态产生影响）
        /// </summary>
        public Action<WorldState> ApplyExpectedEffects;

        public List<BevBase> bevList;
        /// <summary>
        /// 添加对世界的影响函数
        /// </summary>
        /// <returns></returns>
        public PrimitiveTask AddEffects(Action<WorldState> a)
        {
            ApplyEffects = a;
            return this;
        }
        /// <summary>
        /// 添加 对世界状态施加影响 的委托：期望影响（只在规划、检查阶段对世界状态产生影响）
        /// </summary>
        /// <param name="applyExpectedEffects">期望影响</param>
        /// <returns></returns>
        public PrimitiveTask AddExpectedEffects(Action<WorldState> applyExpectedEffects)
        {
            ApplyExpectedEffects = applyExpectedEffects;
            return this;
        }
        /// <summary>
        /// 执行当前行为
        /// </summary>
        /// <returns></returns>
        public virtual HTNResults Execute()
        {
            for (int i = 0; i < bevList.Count; i++)
            {
                if (bevList[i].Execute() == HTNResults.fail)
                {
                    return HTNResults.fail;
                }
            }
            return HTNResults.succeed;
        }

        public PrimitiveTask AddBev(BevBase bev)
        {
            bevList.Add(bev);
            return this;
        }
    }
}
