using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum ShapeType
    {
        /// <summary>
        /// 矩形
        /// </summary>
        Rect,
        /// <summary>
        /// 扇形
        /// </summary>
        Sector,
        /// <summary>
        /// 圆
        /// </summary>
        Circle
    }
    /// <summary>
    /// AI的感知器积累
    /// </summary>
    public abstract class Sensor : MonoBehaviour
    {
        protected DataBase database;
        protected ShapeType shapeType;
        protected void Init(DataBase database)
        {
            this.database = database;
        }

        /// <summary>
        /// 该函数是提供给外界，由特殊的触发器来主动通知
        /// </summary>
        /// <param name="trigger"></param>
        public abstract void Notify(Trigger trigger);
    }
}