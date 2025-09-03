using System.Collections.Generic;
using UnityEngine;

namespace HTN
{
    public class PlanMemento
    {
        protected List<TaskBase> mTaskToProcess = new List<TaskBase>();
        /// <summary>
        /// �������ݳ����ļƻ�
        /// </summary>
        protected List<PrimitiveTask> mFinalPlan = new List<PrimitiveTask>();
        /// <summary>
        /// ��ǰ�����е�Method
        /// </summary>
        protected Method mCurMethod;
        /// <summary>
        /// �������ĸ��Ͻڵ�
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
        /// ��ǰ����״̬
        /// </summary>
        protected WorldState mCurWorldState;
        /// <summary>
        /// ��������״̬������״̬�ĸ�����
        /// </summary>
        protected WorldState mWorkingWorldState;

        /// <summary>
        /// Task���ϵĸ���
        /// </summary>
        protected List<TaskBase> mTaskList = new();
        /// <summary>
        /// ��������Task
        /// </summary>
        protected Stack<TaskBase> mTaskToProcess = new();
        /// <summary>
        /// �����������
        /// </summary>
        protected Queue<PrimitiveTask> mFinalPlan = new();
        ///<summary>
        ///��ǰ����
        ///</summary>
        protected TaskBase mCurTask;

        protected DomainBase mDomain;
        public void Init(WorldState ws, DomainBase domain)
        {
            mCurWorldState = ws;
            mDomain = domain;
            mWorkingWorldState = new WorldState();
            mWorkingWorldState.CopyFrom(ws);
            CopyDomain(domain);
        }
        public void Reset()
        {
            if (mCurWorldState != null)
                mCurWorldState.Reset();
            if (mWorkingWorldState != null)
                mWorkingWorldState.Reset();
            mTaskList.Clear();
            mTaskToProcess.Clear();
        }

        /// <summary>
        /// ����Ԥ��ƻ���
        /// </summary>
        public void BuildPlan()
        {
            if (mCurWorldState == null || mWorkingWorldState == null || mTaskList == null || mTaskList.Count == 0)
            {
                Debug.LogError($"Planner.BuildPlan: 缺失规划组件 - WS:{mCurWorldState != null}, WorkingWS:{mWorkingWorldState != null}, Tasks:{mTaskList?.Count ?? 0}");
                return;
            }
    

            mWorkingWorldState.CopyFrom(mCurWorldState);
            Method vaildmethod = null;
            CompoundTask cTask = null;
            PrimitiveTask pTask = null;
            PlanMemento memento = new PlanMemento();
            mFinalPlan.Clear();  // 清空之前的规划
            mTaskToProcess.Clear();

            mTaskToProcess.Push(mTaskList[0]);

            while (mTaskToProcess.Count > 0)
            {
                mCurTask = mTaskToProcess.Pop();

                if (mCurTask.type == TaskType.Compound)
                {
                    cTask = mCurTask as CompoundTask;
                    vaildmethod = cTask.FindValidMethod(mWorkingWorldState);
                    if (vaildmethod != null && vaildmethod.SubTasks != null)
                    {
                        memento.Save(mTaskToProcess, mFinalPlan, vaildmethod, cTask);
                        InsertTop(vaildmethod.SubTasks);
                    }
                    else
                    {
                        Debug.LogWarning($"复合任务 \"{cTask.Name}\" 找不到有效方法，开始回退...");
                        memento.Recover(ref mTaskToProcess, ref mFinalPlan, ref vaildmethod, ref mCurTask);
                    }
                }
                else
                {
                    pTask = mCurTask as PrimitiveTask;
                    if (pTask.CheckTaskConditions(mWorkingWorldState))
                    {
                        pTask.ApplyEffects(mWorkingWorldState);
                        if (pTask.ApplyExpectedEffects != null)
                            pTask.ApplyExpectedEffects(mWorkingWorldState);
                        mFinalPlan.Enqueue(pTask);
                    }
                    else
                    {
                        Debug.LogWarning($"原子任务 \"{pTask.Name}\" 条件不满足，开始回退...");
                        memento.Recover(ref mTaskToProcess, ref mFinalPlan, ref vaildmethod, ref mCurTask);
                    }
                }
            }
        }

        public void InsertTop(List<TaskBase> tasks)
        {
            if (tasks == null || mTaskToProcess == null)
                return;

            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                if (tasks[i] != null)
                {
                    mTaskToProcess.Push(tasks[i]);
                }
            }
        }
        /// <summary>
        /// ������յĳ��ͼƻ���
        /// </summary>
        /// <returns></returns>
        public Queue<PrimitiveTask> GetFinalTask()
        {
            return mFinalPlan;
        }

        /// <summary>
        /// ��������״̬
        /// </summary>
        /// <param name="ws"></param>
        public void UpdateCurWorldState(WorldState ws)
        {
            mCurWorldState = ws;
            if (mWorkingWorldState != null)
                mWorkingWorldState.CopyFrom(ws);
            else
                Debug.LogWarning("UpdateCurWorldState: mWorkingWorldState is null");
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
