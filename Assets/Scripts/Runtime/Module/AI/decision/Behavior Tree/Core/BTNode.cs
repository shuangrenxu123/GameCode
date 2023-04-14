using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    /// <summary>
    /// 所有节点的父节点
    /// </summary>
    public class BTNode
    {
        public string name;
        public bool isRunning { get; set; }
        public BTNode clearTick { get; set; }
        public BTDataBase database;

        /// <summary>
        /// 激活节点
        /// </summary>
        /// <param name="database"></param>
        public virtual void Activate(BTDataBase database)
        {
            this.database = database;
            if(clearTick != null)
            {
                clearTick.Activate(database);
            }
        }
        /// <summary>
        /// 清除只用关心子节点的清除
        /// </summary>
        public virtual void Clear()
        {

        }

        public virtual BTResult Tick()
        {
            return BTResult.Success;
        }

    }
}
