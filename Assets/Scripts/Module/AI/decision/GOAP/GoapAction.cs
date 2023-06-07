using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    /// <summary>
    /// ���յ�ִ�нڵ㣬
    /// </summary>
    public abstract class GoapAction
    {
        /// <summary>
        /// ǰ������
        /// </summary>
        private HashSet<KeyValuePair<string, object>> preconditions;
        /// <summary>
        /// ִ�е�Ӱ��
        /// </summary>
        private HashSet<KeyValuePair<string, object>> effects;
        /// <summary>
        /// �Ƿ��ڷ�Χ��
        /// </summary>
        private bool inRange = false;
        /// <summary>
        /// �ɱ������ۣ�
        /// </summary>
        public float cost = 1f;
        /// <summary>
        /// Ҫ����������,���ǲ�һ����
        /// </summary>
        public GameObject target = null;
        public GoapAction()
        {
            preconditions = new HashSet<KeyValuePair<string, object>>();
            effects = new HashSet<KeyValuePair<string, object>>();
        }
        /// <summary>
        /// ����
        /// </summary>
        public void doReset()
        {
            inRange = false;
            target = null;
            Reset();
        }
        /// <summary>
        /// ����
        /// </summary>
        protected abstract void Reset();
        /// <summary>
        /// �Ƿ����
        /// </summary>
        /// <returns></returns>
        public abstract bool isDone();
        /// <summary>
        /// ����Ƿ�������У���һ�����е���Ҫ
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public abstract bool CheckProceduralPreconition(HashSet<KeyValuePair<string, object>> state);
        /// <summary>
        /// ����action����Ϊ��Щ����������У�������Ҫ����ֵ
        /// </summary>
        /// <param name="target"></param>
        /// <returns>�Ƿ����гɹ�</returns>
        public abstract bool Perform(GameObject target);
        /// <summary>
        /// �����Ƿ���Ҫ�ڷ�Χ�ڣ�
        /// </summary>
        /// <returns></returns>
        public abstract bool RequiresInRange();
        /// <summary>
        /// �Ƿ��ڷ�Χ��
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

        #region ��Ӻ��Ƴ�  ǰ����Ӱ��
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
        /// ����ǰ������
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
