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
            root = Init();

            if (root.name == null)
            {
                root.name = "Root";
            }
            root.Activate(database);

        }
        private void Update()
        {
            root.Tick();
        }
        protected virtual BTNode Init()
        {
            database = GetComponent<BTDataBase>();
            if (database == null)
            {
                database = gameObject.AddComponent<BTDataBase>();
            }
            return null;    
        }
    }
}
