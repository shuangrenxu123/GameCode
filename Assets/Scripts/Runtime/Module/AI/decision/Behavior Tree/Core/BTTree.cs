using UnityEngine;
namespace BT
{
    public abstract class BTTree : MonoBehaviour
    {
        protected BTNode root = null;

        [HideInInspector]
        public BTDataBase database;
        private void Start()
        {
            Init();
        }
        private void Update()
        {
            root.Tick();
        }
        public abstract void SetNode();
        protected virtual void Init()
        {
            SetNode();
            database = GetComponent<BTDataBase>();
            if (database == null)
            {
                database = gameObject.AddComponent<BTDataBase>();
            }
            if (root != null)
            {
                this.root.Activate(database);
            }
            return;    
        }
    }
}
