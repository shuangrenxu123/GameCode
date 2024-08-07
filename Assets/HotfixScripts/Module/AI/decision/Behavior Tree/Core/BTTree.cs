using UnityEngine;

namespace BT
{
    /// <summary>
    /// 整个行为树的入口函数，具体的行为树入口需要在SetNode中添加好具体的Node与Init则用于实现一些额外逻辑
    /// </summary>
    public abstract class BTTree
    {
        protected BTNode root = null;
        public DataBase database;

        protected float time = 0.02f;
        private float timer = 0;
        public void Update()
        {
            if (timer < time)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
                root.Tick();
            }
        }
        public abstract void SetNode();
        public virtual void Init(DataBase d = null)
        {
            if (d == null)
            {
                database = new DataBase();
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
