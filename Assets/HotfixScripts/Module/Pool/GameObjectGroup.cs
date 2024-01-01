using System.Collections.Generic;
using UnityEngine;

public class GameObjectGroup : MonoBehaviour
{
    /// <summary>
    ///未使用的资源队列 
    /// </summary>
    public List<PoolObject> pool;
    /// <summary>
    /// 使用了的队列
    /// </summary>
    public List<PoolObject> usePool;
    private GameObject Go;
    /// <summary>
    /// 用完后不销毁
    /// </summary>
    //public bool DontDestory;
    /// <summary>
    /// 总数
    /// </summary>
    private int capcity;
    public void Init(string name, GameObject prefab, int size, GameObject parent)
    {
        capcity = size;
        pool = new List<PoolObject>(capcity * 2);
        Go = prefab;
        usePool = new List<PoolObject>(capcity * 2);
        for (int i = 0; i < capcity; i++)
        {
            GameObject a = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            a.transform.parent = parent.transform;
            a.GetComponent<PoolObject>().group = this;
            a.SetActive(false);
            PoolObject b = a.GetComponent<PoolObject>();
            pool.Add(b);
        }
    }
    /// <summary>
    /// 取一个物体
    /// </summary>
    public PoolObject GetGameobject(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            AddObject();
        }
        var temp = pool[0];
        pool.RemoveAt(0);
        usePool.Add(temp);
        Debug.Log(pool.Count);
        temp.gameObject.transform.SetPositionAndRotation(position, rotation);
        temp.gameObject.SetActive(true);
        temp.Init();
        return temp;
    }
    /// <summary>
    /// 回收物体
    /// </summary>
    public void Restore()
    {
        PoolObject c = usePool[0];
        usePool.RemoveAt(0);
        c.gameObject.SetActive(false);
        pool.Add(c);
    }
    public void Restore(PoolObject go)
    {
        usePool.Remove(go);
        pool.Add(go);
        go.gameObject.SetActive(false);
    }
    /// <summary>
    /// 销毁对象池
    /// </summary>
    public void Destory()
    {

    }
    private void AddObject()
    {
        Debug.Log("添加总数量");
        for (int i = 0; i < 5; i++)
        {
            GameObject a = Instantiate(Go, Vector3.zero, Quaternion.identity);
            a.SetActive(false);
            PoolObject b = a.GetComponent<PoolObject>();
            pool.Add(b);
        }
        capcity += 5;
    }
}
