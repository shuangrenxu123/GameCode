using System;
using System.Collections.Generic;
namespace HTN
{
    public enum HTNResults
    {
        /// <summary>
        /// ִ�гɹ�
        /// </summary>
        succeed,
        /// <summary>
        /// ִ��ʧ��
        /// </summary>
        fail,
    }
    public abstract class PrimitiveTask : TaskBase
    {
        public PrimitiveTask(DomainBase domain, string name, TaskType t, List<HTNCondition> c = null) : base(domain, name, t, c)
        {
            bevList = new List<BevBase>();
        }
        /// <summary>
        /// ִ�к���������ɵ�Ӱ��
        /// </summary>
        public Action<WorldState> ApplyEffects;
        /// <summary>
        /// ����Ӱ�죨ֻ�ڹ滮�����׶ζ�����״̬����Ӱ�죩
        /// </summary>
        public Action<WorldState> ApplyExpectedEffects;

        public List<BevBase> bevList;
        /// <summary>
        /// ���Ӷ������Ӱ�캯��
        /// </summary>
        /// <returns></returns>
        public PrimitiveTask AddEffects(Action<WorldState> a)
        {
            ApplyEffects = a;
            return this;
        }
        /// <summary>
        /// ���� ������״̬ʩ��Ӱ�� ��ί�У�����Ӱ�죨ֻ�ڹ滮�����׶ζ�����״̬����Ӱ�죩
        /// </summary>
        /// <param name="applyExpectedEffects">����Ӱ��</param>
        /// <returns></returns>
        public PrimitiveTask AddExpectedEffects(Action<WorldState> applyExpectedEffects)
        {
            ApplyExpectedEffects = applyExpectedEffects;
            return this;
        }
        /// <summary>
        /// ִ�е�ǰ��Ϊ
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
            if (bevList != null && bev != null)
                bevList.Add(bev);
            return this;
        }
    }
}
