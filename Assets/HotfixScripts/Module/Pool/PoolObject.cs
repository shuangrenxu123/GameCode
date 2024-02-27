using UnityEngine;
namespace ObjectPool
{


    public class PoolObject : MonoBehaviour
    {
        public GameObjectGroup group;
        /// <summary>
        /// 添加进池子的时候调用一次
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// 出队时候调用
        /// </summary>
        public virtual void Push()
        {

        }
        /// <summary>
        /// 回收时候调用
        /// </summary>
        public virtual void Pull()
        {

        }
    }
}