using UnityEngine;
namespace FindPath
{
    public class Cell
    {
        /// <summary>
        /// 单元格坐标
        /// </summary>
        public Vector2 position;
        /// <summary>
        /// 指向方向
        /// </summary>
        public Vector2 direction = Vector2.zero;

        public float distance = 65535;
        /// <summary>
        /// 是否是障碍物
        /// </summary>
        public bool UnPassable = false;
        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }
}