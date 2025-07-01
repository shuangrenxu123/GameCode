using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPool
{
    public class ReferenceManager : ModuleSingleton<ReferenceManager>, IModule
    {
        private static readonly Dictionary<Type, ReferenceCollector> _collectors = new Dictionary<Type, ReferenceCollector>();
        public void OnCreate(object createParam)
        {

        }

        public void OnUpdate()
        {

        }
        /// <summary>
        /// 对象池初始容量
        /// </summary>
        public int InitCapacity { get; set; } = 60;

        /// <summary>
        /// 对象池的数量
        /// </summary>
        public int Count
        {
            get
            {
                return _collectors.Count;
            }
        }

        /// <summary>
        /// 清除所有对象池
        /// </summary>
        public void ClearAll()
        {
            _collectors.Clear();
        }

        /// <summary>
        /// 申请引用对象
        /// </summary>
        public T Spawn<T>() where T : class, IReferenceObject
        {
            var type = typeof(T);
            if (_collectors.ContainsKey(type) == false)
            {
                _collectors.Add(type, new ReferenceCollector<T>(InitCapacity));
            }
            return _collectors[type].Spawn() as T;
        }
        /// <summary>
        /// 回收引用对象
        /// </summary>
        public void Release<T>(T item) where T : class, IReferenceObject
        {
            Type type = typeof(T);
            if (_collectors.ContainsKey(type) != false)
            {
                _collectors[type].Release(item);
            }
        }

        /// <summary>
        /// 批量回收列表集合
        /// </summary>
        public void Release<T>(List<T> items) where T : class, IReferenceObject, new()
        {
            Type type = typeof(T);
            if (_collectors.ContainsKey(type) != false)
            {

                for (int i = 0; i < items.Count; i++)
                {
                    _collectors[type].Release(items[i]);
                }
            }
        }

        /// <summary>
        /// 批量回收数组集合
        /// </summary>
        public void Release<T>(T[] items) where T : class, IReferenceObject, new()
        {
            Type type = typeof(T);
            if (_collectors.ContainsKey(type) != false)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    _collectors[type].Release(items[i]);
                }
            }
        }

    }
    public abstract class ReferenceCollector
    {
        public abstract object Spawn();
        public abstract void Release(object item);
    }

    public class ReferenceCollector<T> : ReferenceCollector
         where T : class, IReferenceObject
    {
        private readonly HashSet<T> _collector;

        /// <summary>
        /// 引用类型
        /// </summary>
        public Type ClassType { private set; get; }

        /// <summary>
        /// 内部缓存总数
        /// </summary>
        public int Count
        {
            get { return _collector.Count; }
        }

        /// <summary>
        /// 外部使用总数
        /// </summary>
        public int SpawnCount { private set; get; }


        public ReferenceCollector(int capacity)
        {
            ClassType = typeof(T);

            // 创建缓存池
            _collector = new HashSet<T>(capacity);
        }

        /// <summary>
        /// 申请引用对象
        /// </summary>
        public T SpawnInternal()
        {
            T item;
            if (_collector.Count > 0)
            {
                // HashSet无顺序，取第一个
                item = _collector.First();
                _collector.Remove(item);
            }
            else
            {
                item = Activator.CreateInstance(ClassType) as T;
            }

            item.OnInit();

            SpawnCount++;
            return item;
        }

        /// <summary>
        /// 回收引用对象
        /// </summary>
        public void Release(T item)
        {
            if (item == null)
                return;

            if (item.GetType() != ClassType)
                throw new Exception($"Invalid type {item.GetType()}");

            if (_collector.Contains(item))
            {
                UnityEngine.Debug.LogError($"The item {item.GetType()} already exists.");
                return;
            }
            item.OnRelease();
            SpawnCount--;
            _collector.Add(item);
        }

        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            _collector.Clear();
            SpawnCount = 0;
        }

        public override object Spawn()
        {
            return SpawnInternal();
        }

        public override void Release(object item)
        {
            Release(item as T);
        }
    }


    public interface IReferenceObject
    {
        public void OnInit();
        public void OnRelease();
    }
}