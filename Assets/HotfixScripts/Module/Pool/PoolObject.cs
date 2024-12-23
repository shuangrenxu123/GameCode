using UnityEngine;
namespace ObjectPool
{
    public class PoolObject : MonoBehaviour
    {
        public GameObjectGroup group;
        /// <summary>
        /// ���ӽ����ӵ�ʱ�����һ��
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// ����ʱ�����
        /// </summary>
        public virtual void Push()
        {

        }
        /// <summary>
        /// ����ʱ�����
        /// </summary>
        public virtual void Pull()
        {

        }
        public void Release()
        {
            group.Restore(this);
        }
    }
}