using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public T Spawn<T>() where T : class
    {
        var type = typeof(T);
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, InitCapacity));
        }
        return _collectors[type].Spawn<T>();
    }
    public T Spawn<T>(int capacity) where T : class
    {
        var type = typeof(T);
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, capacity));
        }
        return _collectors[type].Spawn<T>();
    }
    public object Spawn(Type type)
    {
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, InitCapacity));
        }
        return _collectors[type].Spawn(); ;
    }
    /// <summary>
    /// 回收引用对象
    /// </summary>
    public void Release(object item)
    {
        Type type = item.GetType();
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, InitCapacity));
        }
        _collectors[type].Release(item);
    }

    /// <summary>
    /// 批量回收列表集合
    /// </summary>
    public void Release<T>(List<T> items) where T : class, new()
    {
        Type type = typeof(T);
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, InitCapacity));
        }

        for (int i = 0; i < items.Count; i++)
        {
            _collectors[type].Release(items[i]);
        }
    }

    /// <summary>
    /// 批量回收数组集合
    /// </summary>
    public void Release<T>(T[] items) where T : class, new()
    {
        Type type = typeof(T);
        if (_collectors.ContainsKey(type) == false)
        {
            _collectors.Add(type, new ReferenceCollector(type, InitCapacity));
        }

        for (int i = 0; i < items.Length; i++)
        {
            _collectors[type].Release(items[i]);
        }
    }

}

public class ReferenceCollector
{
    private readonly Stack<object> _collector;

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


    public ReferenceCollector(Type type, int capacity)
    {
        ClassType = type;

        // 创建缓存池
        _collector = new Stack<object>(capacity);

    }

    /// <summary>
    /// 申请引用对象
    /// </summary>
    public T Spawn<T>() where T : class
    {
        object item;
        if (_collector.Count > 0)
        {
            item = _collector.Pop();
        }
        else
        {
            item = Activator.CreateInstance(ClassType) as T;
        }

        SpawnCount++;
        return item as T;
    }
    public  object Spawn()
    {
        object item;
        if (_collector.Count > 0)
        {
            item = _collector.Pop();
        }
        else
        {
            item = Activator.CreateInstance(ClassType);
        }

        SpawnCount++;
        return item;
    }
    /// <summary>
    /// 回收引用对象
    /// </summary>
    public void Release(object item)
    {
        if (item == null)
            return;

        if (item.GetType() != ClassType)
            throw new Exception($"Invalid type {item.GetType()}");

        if (_collector.Contains(item))
        {
            //throw new Exception($"The item {item.GetType()} already exists.");
            UnityEngine.Debug.LogError($"The item {item.GetType()} already exists.");
            return; 
        }
        SpawnCount--;
        _collector.Push(item);
    }

    /// <summary>
    /// 清空集合
    /// </summary>
    public void Clear()
    {
        _collector.Clear();
        SpawnCount = 0;
    }
}