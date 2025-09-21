using UnityEngine;

namespace BT
{
    /// <summary>
    /// 整个行为树的入口函数，具体的行为树入口需要在SetNode中添加好具体的Node与Init则用于实现一些额外逻辑
    /// </summary>
    public abstract class BTTree<TKey, TValue>
    {
        protected BTNode<TKey, TValue> root = null;
        public DataBase<TKey, TValue> database;

        public void Update()
        {
            root.Tick();
        }
        public abstract void SetNode();
        public virtual void Init(DataBase<TKey, TValue> d = null)
        {
            if (d == null)
            {
                database = new DataBase<TKey, TValue>();
            }
            else
            {
                database = d;
            }

            SetNode();
            root?.Activate(database);
        }
    }
}
