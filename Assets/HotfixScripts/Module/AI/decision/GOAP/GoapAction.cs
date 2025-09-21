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
        public GoapAction()
        {
            preconditions = new();
            effects = new();
        }


        public bool IsDone()
        {
            return executed;
        }

        protected abstract void Reset();

        public abstract bool CheckProceduralPreCondition(Dictionary<T, V> state);


        public virtual void PlanEnter()
        {
            executed = false;
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

        }
    }
}
