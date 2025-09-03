using System.Collections.Generic;
using UnityEngine;
namespace HTN
{
    public class Method
    {
        public int id;
        // ����д��һ���������Կ����������
        public HTNCondition cond;
        /// <summary>
        /// ��Method�µ��ӷ���,��Ϊ�п��ܻ��з�֧������Ӧ����дTaskbase
        /// </summary>
        public List<TaskBase> SubTasks;
        public Method(int id, HTNCondition c = null)
        {
            cond = c;
            this.id = id;
            SubTasks = new List<TaskBase>();
        }
        public Method AddTask(TaskBase task)
        {
            if (task != null)
                SubTasks.Add(task);
            return this;
        }
    }

    public class CompoundTask : TaskBase
    {
        public List<Method> methods;
        public CompoundTask(DomainBase domain, string name, TaskType type, List<HTNCondition> c = null) : base(domain, name, type, c)
        {
            methods = new List<Method>();
        }
        public void Reset()
        {
            methods.Clear();
            conds = null;
        }

        public CompoundTask AddMethod(Method v)
        {
            if (v != null)
            {
                methods.Add(v);
            }
            return this;
        }

        /// <summary>
        /// ��������״̬��Ȼ�����ĸ�method�Ǹ��Ͻ����Ȼ�󷵻ض�Ӧ��method
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public Method FindValidMethod(WorldState ws)
        {
            if (methods == null || ws == null)
            {
                Debug.LogError("FindValidMethod: 方法列表或世界状态为空");
                return null;
            }

            foreach (var i in methods)
            {
                if (i == null || i.cond == null)
                {
                    continue;
                }

                // 检查条件 - 除了在StaminaCondition中保持必要日志，其他一律移除详细调试打印
                if (i.cond.Check(ws))
                {
                    return i;
                }
            }

            Debug.LogError($"复合任务 '{Name}' 没有找到任何有效的方法！所有 {methods.Count} 个方法条件都不满足。");
            return null;
        }
    }
}
