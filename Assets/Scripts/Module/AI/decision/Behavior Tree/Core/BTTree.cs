namespace BT
{
    /// <summary>
    /// 整个行为树的入口函数，具体的行为树入口需要在SetNode中添加好具体的Node与Init则用于实现一些额外逻辑
    /// </summary>
    public abstract class BTTree
    {
        public Enemy enemy;
        protected BTNode root = null;
        public DataBase database;
        public void Update()
        {
            root.Tick();
        }
        public abstract void SetNode();
        public virtual void Init(Enemy enemy, Enemy tree, DataBase d = null)
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
            root?.Activate(database, tree);
        }
    }
}
