using System.Collections.Generic;

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
        /// �������Task
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
        /// ����Ԥ��ƻ���
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
