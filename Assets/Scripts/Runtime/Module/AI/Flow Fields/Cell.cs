using UnityEngine;
namespace FindPath
{
    public class Cell
    {
        /// <summary>
        /// ��Ԫ������
        /// </summary>
        public Vector2 position;
        /// <summary>
        /// ָ����
        /// </summary>
        public Vector2 direction = Vector2.zero;

        public float distance = 65535;
        /// <summary>
        /// �Ƿ����ϰ���
        /// </summary>
        public bool UnPassable = false;
        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }
}