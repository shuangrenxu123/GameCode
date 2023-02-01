using UnityEngine;
namespace BT
{
    public abstract class BTTree : MonoBehaviour
    {
        protected BTNode root = null;

        [HideInInspector]
        public BTDataBase database;
        [HideInInspector]
        public bool isRunning = true;

        /// <summary>
        /// 在数据库是否重置
        /// </summary>
        public const string RESET = "Reset";
        public static int reserid;


        public void Awake()
        {
            Init();
            root.Activate(database);
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }
            if (database.GetData<bool>(RESET))
            {
                Reset();
                database.SetData<bool>(RESET, false);
            }
            if (root.Evaluate())
            {
                root.Tick();
            }
        }
        void OnDestroy()
        {
            if (root != null)
            {
                root.Clear();
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Init()
        {
            database = GetComponent<BTDataBase>();
            if (database == null)
            {
                database = gameObject.AddComponent<BTDataBase>();
            }
            reserid = database.GetDataId(RESET);
            database.SetData(reserid, false);
        }
        protected void Reset()
        {
            root.Clear();
        }
    }
}
