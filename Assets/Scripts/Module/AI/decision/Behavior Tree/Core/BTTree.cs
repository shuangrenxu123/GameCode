using UnityEngine;
namespace BT
{
    public abstract class BTTree
    {
        public Enemy enemy;
        protected BTNode root = null;
        public BTDataBase database;
        public void Update()
        {
            root.Tick();
        }
        public abstract void SetNode();
        public virtual void Init(Enemy enemy, EnemyAnimatorHandle anim, Enemy tree,BTDataBase d = null)
        {
            if (d == null)
            {
                database = new BTDataBase();
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
