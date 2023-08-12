using System.Collections.Generic;

namespace HTN
{
    public class PlanMemento
    {
        protected List<TaskBase> mTaskToProcess = new List<TaskBase>();
        /// <summary>
        /// 最终推演出来的计划
        /// </summary>
        protected List<PrimitiveTask> mFinalPlan = new List<PrimitiveTask>();
        /// <summary>
        /// 当前推演中的Method
        /// </summary>
        protected Method mCurMethod;
        /// <summary>
        /// 所隶属的复合节点
        /// </summary>
        protected TaskBase mBelongCompound;

        public TaskBase BelongCompound => mBelongCompound;
        public void Reset()
        {
            mTaskToProcess.Clear();
            mFinalPlan.Clear();
            mCurMethod = null;
            mBelongCompound = null;
        }

        public void Save(Stack<TaskBase> taskToProcess, Queue<PrimitiveTask> finalTask, Method method, TaskBase BelongCompound)
        {
            foreach (var i in taskToProcess)
            {
                mTaskToProcess.Add(i);
            }
            foreach (var i in finalTask)
            {
                mFinalPlan.Add(i);
            }
            mCurMethod = method;
            mBelongCompound = BelongCompound;
        }
        public void Recover(ref Stack<TaskBase> taskToProcess, ref Queue<PrimitiveTask> finalplan, ref Method method, ref TaskBase belongCompound)
        {
            taskToProcess.Clear();
            finalplan.Clear();
            for (int i = mTaskToProcess.Count - 1; i >= 0; i--)
            {
                taskToProcess.Push(mTaskToProcess[i]);
            }
            for (int i = 0; i < mFinalPlan.Count; i++)
            {
                finalplan.Enqueue(mFinalPlan[i]);
            }
            method = mCurMethod;
            belongCompound = mBelongCompound;
        }
    }
    public class Planner
    {
        /// <summary>
        /// 当前世界状态
        /// </summary>
        protected WorldState mCurWorldState;
        /// <summary>
        /// 工作世界状态（世界状态的副本）
        /// </summary>
        protected WorldState mWorkingWorldState;

        /// <summary>
        /// Task集合的副本
        /// </summary>
        protected List<TaskBase> mTaskList = new();
        /// <summary>
        /// 待处理的Task
        /// </summary>
        protected Stack<TaskBase> mTaskToProcess = new();
        /// <summary>
        /// 最终任务队列
        /// </summary>
        protected Queue<PrimitiveTask> mFinalPlan = new();
        ///<summary>
        ///当前任务
        ///</summary>
        protected TaskBase mCurTask;

        protected DomainBase mDomain;
        public void Init(WorldState ws, DomainBase domain)
        {
            mCurWorldState = ws;
            mDomain = domain;
            mWorkingWorldState.CopyFrom(ws);
            CopyDomain(domain);
        }
        public void Reset()
        {
            mCurWorldState.Reset();
            mWorkingWorldState.Reset();
            mTaskList.Clear();
            mTaskToProcess.Clear();
        }

        /// <summary>
        /// 生成预测计划表
        /// </summary>
        public void BuildPlan()
        {
            mWorkingWorldState.CopyFrom(mCurWorldState);
            Method vaildmethod = null;
            CompoundTask cTask = null;
            PrimitiveTask pTask = null;
            PlanMemento memento = new PlanMemento();
            mTaskToProcess.Push(mTaskList[0]);
            while (mTaskToProcess.Count > 0)
            {
                mCurTask = mTaskToProcess.Pop();
                if (mCurTask.type == TaskType.Compound)
                {
                    cTask = mCurTask as CompoundTask;
                    vaildmethod = cTask.FindValidMethod(mWorkingWorldState);
                    if (vaildmethod != null)
                    {
                        memento.Save(mTaskToProcess, mFinalPlan, vaildmethod, cTask);
                        InsertTop(vaildmethod.SubTasks);
                    }

                    else
                    {
                        memento.Recover(ref mTaskToProcess, ref mFinalPlan, ref vaildmethod, ref mCurTask);
                    }
                }
                else
                {
                    pTask = mCurTask as PrimitiveTask;
                    if (pTask.cond.Check(mWorkingWorldState))
                    {
                        pTask.ApplyEffects(mWorkingWorldState);
                        pTask.ApplyExpectedEffects(mWorkingWorldState);
                        mFinalPlan.Enqueue(pTask);
                    }
                    else
                    {
                        memento.Recover(ref mTaskToProcess, ref mFinalPlan, ref vaildmethod, ref mCurTask);
                    }
                }
            }
        }

        public void InsertTop(List<TaskBase> tasks)
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                mTaskToProcess.Push(tasks[i]);
            }
        }
        /// <summary>
        /// 获得最终的成型计划表
        /// </summary>
        /// <returns></returns>
        public Queue<PrimitiveTask> GetFinalTask()
        {
            return mFinalPlan;
        }

        /// <summary>
        /// 更新世界状态
        /// </summary>
        /// <param name="ws"></param>
        public void UpdateCurWorldState(WorldState ws)
        {
            mCurWorldState = ws;
            mWorkingWorldState.CopyFrom(ws);
        }

        public void CopyDomain(DomainBase domain)
        {
            mTaskList.Clear();
            for (int i = 0; i < domain.TaskList.Count; i++)
            {
                mTaskList.Add(domain.TaskList[i]);
            }
        }
    }
}
