using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum ShapeType
    {
        /// <summary>
        /// ����
        /// </summary>
        Rect,
        /// <summary>
        /// ����
        /// </summary>
        Sector,
        /// <summary>
        /// Բ
        /// </summary>
        Circle
    }
    /// <summary>
    /// AI�ĸ�֪������
    /// </summary>
    public abstract class Sensor : MonoBehaviour
    {
        protected DataBase<string, object> database;
        protected ShapeType shapeType;
        protected void Init(DataBase<string, object> database)
        {
            this.database = database;
        }

        /// <summary>
        /// �ú������ṩ����磬������Ĵ�����������֪ͨ
        /// </summary>
        /// <param name="trigger"></param>
        public abstract void Notify(Trigger trigger);
    }
}