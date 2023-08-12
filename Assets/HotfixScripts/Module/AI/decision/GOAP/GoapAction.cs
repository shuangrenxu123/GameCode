using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// 最终的执行节点，
    /// </summary>
    public abstract class GoapAction
    {
        /// <summary>
        /// 前置条件
        /// </summary>
        private HashSet<KeyValuePair<string, object>> preconditions;
        /// <summary>
        /// 执行的影响
        /// </summary>
        private HashSet<KeyValuePair<string, object>> effects;
        /// <summary>
        /// 是否在范围内
        /// </summary>
        private bool inRange = false;
        /// <summary>
        /// 成本（代价）
        /// </summary>
        public float cost = 1f;
        /// <summary>
        /// 要操作的物体,但是不一定有
        /// </summary>
        public GameObject target = null;
        public GoapAction()
        {
            preconditions = new HashSet<KeyValuePair<string, object>>();
            effects = new HashSet<KeyValuePair<string, object>>();
        }
        /// <summary>
        /// 重置
        /// </summary>
        public void doReset()
        {
            inRange = false;
            target = null;
            Reset();
        }
        /// <summary>
        /// 重置
        /// </summary>
        protected abstract void Reset();
        /// <summary>
        /// 是否结束
        /// </summary>
        /// <returns></returns>
        public abstract bool isDone();
        /// <summary>
        /// 检查是否可以运行，不一定所有的需要
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public abstract bool CheckProceduralPreconition(HashSet<KeyValuePair<string, object>> state);
        /// <summary>
        /// 运行action，因为有些情况会打断运行，所以需要返回值
        /// </summary>
        /// <param name="target"></param>
        /// <returns>是否运行成功</returns>
        public abstract bool Perform(GameObject target);
        /// <summary>
        /// 物体是否需要在范围内，
        /// </summary>
        /// <returns></returns>
        public abstract bool RequiresInRange();
        /// <summary>
        /// 是否在范围内
        /// </summary>
        /// <returns></returns>
        public bool IsInRange()
        {
            return inRange;
        }
        public void SetInRange(bool inrange)
        {
            inRange = inrange;
        }

        #region 添加和移除  前置与影响
        public void AddPrecondition(string key, object value)
        {
            preconditions.Add(new KeyValuePair<string, object>(key, value));
        }
        public void RemovePrecondition(string key)
        {
            KeyValuePair<string, object> temp = default;
            foreach (var i in preconditions)
            {
                if (i.Key == key)
                {
                    temp = i;
                    break;
                }
            }
            preconditions.Remove(temp);
        }
        public void AddEffect(string key, object value)
        {
            effects.Add(new KeyValuePair<string, object>(key, value));
        }
        public void RemoveEffect(string key)
        {
            KeyValuePair<string, object> temp = default;
            foreach (var i in effects)
            {
                if (i.Key == key)
                {
                    temp = i;
                    break;
                }
            }
            effects.Remove(temp);
        }
        #endregion
        /// <summary>
        /// 返回前置条件
        /// </summary>
        public HashSet<KeyValuePair<string, object>> Preconditions
        {
            get
            {
                return preconditions;
            }
        }
        public HashSet<KeyValuePair<string, object>> Effects
        {
            get
            {
                return effects;
            }
        }
    }
}
