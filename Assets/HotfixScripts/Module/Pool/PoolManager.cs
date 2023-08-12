using System.Collections.Generic;
using UnityEngine;

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
    public void CreatePool(PoolInfo info)
    {
        if (PoolMap.ContainsKey(info.name))
        {
            Debug.LogError(info.name + "已经存在");
        }
        GameObjectGroup pool = new GameObjectGroup(info.name, info.prefab, info.size);
        PoolMap[info.name] = pool;
    }
    public GameObjectGroup GetPool(string name)
    {
        if (PoolMap.ContainsKey(name))
            return PoolMap[name];
        else
            Debug.LogError(name + "不存在");
        return null;

    }

    public GameObject GetGameObjectToPool(string Poolname, Vector3 position, Quaternion rotation)
    {
        if (PoolMap.ContainsKey(Poolname))
        {
            GameObject Go = PoolMap[Poolname].GetGameobject(position, rotation);
            return Go;
        }
        Debug.LogError("对象池不存在");
        return null;
    }

    public void ReturnObjectToPool(string name)
    {
        PoolMap[name].Restore();
    }
}
public class PoolInfo
{
    public PoolInfo(string n, int s, GameObject p)
    {
        prefab = p;
        size = s;
        name = n;
    }
    public string name;
    public int size;
    public GameObject prefab;
}
