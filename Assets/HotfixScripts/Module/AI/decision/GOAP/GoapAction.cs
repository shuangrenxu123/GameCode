using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    ///  基础的GOAP行为
    /// </summary>
    public abstract class GoapAction<T, V>
    {
        /// <summary>
        /// Action 在单个 Agent 中唯一占用且生命周期内保持不变的位标识。
        /// 必须非零，并且只能包含一个置位 bit。
        /// </summary>
        public abstract ulong ActionMask { get; }

        /// <summary>
        /// 行为所需要的前置条件
        /// </summary>
        protected Dictionary<T, V> preconditions;

        /// <summary>
        /// 行为执行完毕后造成的影响
        /// </summary>
        protected Dictionary<T, V> effects;

        public Dictionary<T, V> Preconditions
        {
            get
            {
                return preconditions;
            }
        }
        public Dictionary<T, V> Effects
        {
            get
            {
                return effects;
            }
        }
        /// <summary>
        /// 行为代价
        /// </summary>
        public float cost = 1f;

        public string name = "";
        protected bool executed = false;
        protected bool running = false;
        public GoapAction()
        {
            preconditions = new();
            effects = new();
        }


        public bool IsDone => executed;
        public bool Running => running;

        protected abstract void Reset();

        public abstract bool CheckProceduralPreCondition(Dictionary<T, V> state);


        public virtual void PlanEnter()
        {
            executed = false;
            running = true;
        }

        public virtual void PlanExecute()
        {
            if (!executed)
            {
                Debug.Log("Executing: " + name);
            }
        }
        public virtual void PlanExit()
        {
            Reset();
            running = false;

        }
    }
}
