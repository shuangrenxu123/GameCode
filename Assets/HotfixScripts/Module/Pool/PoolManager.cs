using System.Collections.Generic;
using UnityEngine;

namespace ObjectPool
{
    public class PoolManager : ModuleSingleton<PoolManager>, IModule
    {
        private Dictionary<string, GameObjectGroup> PoolMap = new Dictionary<string, GameObjectGroup>();
        public PoolInfo[] PoolInfo;
        private GameObject root;
        public void OnCreate(object createParam)
        {
            root = new GameObject("[Pool]");
            root.transform.position = Vector3.zero;
            UnityEngine.GameObject.DontDestroyOnLoad(root);
        }
        public void OnUpdate()
        {

        }
        /// <summary>
        /// 创建一个池子
        /// </summary>
        /// <param name="name">对象池</param>
        /// <param name="capcity">申请的空间</param>
        /// <returns></returns>
        public void CreatePool(PoolInfo info, bool destory = true)
        {
            if (PoolMap.ContainsKey(info.name))
            {
                Debug.LogError(info.name + "已经存在");
                return;
            }
            var group = new GameObject(info.name);
            var c = group.AddComponent<GameObjectGroup>();
            c.Init(info.name, info.prefab, info.size, group);
            PoolMap[info.name] = c;
            if (!destory)
            {
                Object.DontDestroyOnLoad(group);
            }
        }
        public GameObjectGroup GetPool(string name)
        {
            if (PoolMap.ContainsKey(name))
                return PoolMap[name];
            else
                Debug.LogError(name + "不存在");
            return null;

        }
        /// <summary>
        /// 从池子中获得一个物体
        /// </summary>
        /// <param name="poolName">池子名称</param>
        /// <param name="position">物体坐标</param>
        /// <param name="rotation">旋转角度</param>
        /// <returns></returns>
        public T GetGameObjectToPool<T>(string poolName, Vector3 position, Quaternion rotation) where T : PoolObject
        {
            if (PoolMap.ContainsKey(poolName))
            {
                PoolObject Go = PoolMap[poolName].GetGameObject(position, rotation);
                return Go as T;
            }
            Debug.LogError("对象池不存在");
            return null;
        }
        //回收存在时间最长的物体返回池子
        public void ReturnObjectToPool(string name)
        {
            PoolMap[name].Restore();
        }

        public void ReturnObjectToPool(string name, PoolObject go)
        {
            PoolMap[name].Restore(go);
        }
    }
    public class PoolInfo
    {
        public PoolInfo(string name, int size, GameObject prefab)
        {
            this.prefab = prefab;
            this.size = size;
            this.name = name;
        }
        public string name;
        public int size;
        public GameObject prefab;
    }
}